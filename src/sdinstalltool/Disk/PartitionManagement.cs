using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using SDInstallTool.Helpers;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace SDInstallTool
{
    #region Types

    public enum PartitionTypeEnum: byte
    {
        Empty = 0x00,
        ExFAT = 0x07,
        FAT32CHS = 0x0B,
        FAT32LBA = 0x0C,
        Linux = 0x83,
        SoCPreloader = 0xA2,

        Unknown = 0xFF          // Any other partition types will be treated as unknown
    }

    public struct PartitionInfo
    {
        public uint offsetStart;
        public uint length;
        public PartitionTypeEnum type;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 512)]
    public struct MBR
    {
        public MBR(uint signature): this()
        {
            reserved1 = new byte[440];
            this.copyProtected = 0x0000;
            this.signature = signature;

            this.bootSignature1 = 0x55;
            this.bootSignature2 = 0xAA;
        }

        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 440)]
        public byte[] reserved1;

        [FieldOffset(440)]
        public uint signature;

        [FieldOffset(444)]
        public ushort copyProtected;

        #region Union partitions array vs single partition access

        /*
         * C# does not support arrays unaligned to x4
        [FieldOffset(446)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public MBRPartitionEntry[] partitions;
        */

        [FieldOffset(446)]
        public MBRPartitionEntry partition1;

        [FieldOffset(462)]
        public MBRPartitionEntry partition2;

        [FieldOffset(478)]
        public MBRPartitionEntry partition3;

        [FieldOffset(494)]
        public MBRPartitionEntry partition4;

        #endregion

        [FieldOffset(510)]
        public byte bootSignature1;

        [FieldOffset(511)]
        public byte bootSignature2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
    public struct MBRPartitionEntry
    {
        public byte status;
        public byte chsAddress1;
        public byte chsAddress2;
        public byte chsAddress3;

        public byte partitionType;
        public byte chsLastAddress1;
        public byte chsLastAddress2;
        public byte chsLastAddress3;

        public uint lbaFirstSector;
        public uint partitionLengthSectors;
    }

    #endregion

    public static partial class PartitionManagement
    {
        #region Constants

        static readonly Regex PatternGUID = new Regex(@"^.*([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}).*$", RegexOptions.Compiled);

        static readonly string DiskFormatPattern = "{0} /FS:exFAT /A:32K /Y /V:MiSTer_Data /Q";

        #endregion

        #region Win32 Interop methods

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteVolumeMountPoint(string mountPoint);

        #endregion

        #region WMI methods

        public static String getVolumeGUID(String volumeName)
        {
            String result = String.Empty;

            var queryObj = getDiskVolumeWMI(volumeName);

            result = getVolumeGUID(queryObj);

            return result;
        }

        public static String getVolumeGUID(ManagementObject obj)
        {
            String result = String.Empty;
            String mountPoint = String.Empty;

            // Try to ask WMI about Volume GUID first
            if (obj != null)
            {
                var deviceID = obj["DeviceID"].ToString();

                if (!String.IsNullOrEmpty(deviceID))
                {
                    var match = PatternGUID.Match(deviceID);
                    if (match.Success)
                    {
                        var guid = match.Groups[1].Value;
                        result = String.Format("\\\\?\\Volume{{{0}}}", guid);
                    }
                }

                mountPoint = obj["DriveLetter"].ToString();
            }

            // If WMI has no information about volume GUID - use WinAPI method
            if (String.IsNullOrEmpty(result) && !String.IsNullOrEmpty(mountPoint))
            {
                result = DiskManagement.getVolumeNameForVolumeMountPointWin32(mountPoint);
            }

            return result;
        }

        #endregion

        #region MBR operations

        /// <summary>
        /// Create MBR sector (512 bytes) with partitions specified in a parameter list
        /// </summary>
        /// <param name="partitions">List of partition descriptors</param>
        /// <returns>512 bytes array with MBR disk sector content</returns>
        public static byte[] createMBRSector(List<PartitionInfo> partitions)
        {
            byte[] result = new byte[512];

            if (partitions != null && partitions.Count > 0)
            {
                MBR mbr = new MBR(getDiskSignature());

                int idx = 0;
                foreach (PartitionInfo partition in partitions)
                {
                    // There can be only 4 primary partitions in MBR
                    if (idx >= 4)
                        break;

                    MBRPartitionEntry mbrPartition = new MBRPartitionEntry();
                    mbrPartition.partitionType = Convert.ToByte(partition.type);
                    mbrPartition.lbaFirstSector = partition.offsetStart;
                    mbrPartition.partitionLengthSectors = partition.length;

                    // Since CLR does not allow arrays not aligned to x4 - workaround to address particular instances
                    switch (idx)
                    {
                        case 0:
                            mbr.partition1 = mbrPartition;
                            break;
                        case 1:
                            mbr.partition2 = mbrPartition;
                            break;
                        case 2:
                            mbr.partition3 = mbrPartition;
                            break;
                        case 3:
                            mbr.partition4 = mbrPartition;
                            break;
                    }

                    idx++;
                }

                result = getBytes(mbr);
            }

            return result;
        }

        /// <summary>
        /// Transform 512 bytes of MBR data to list of partition info blocks
        /// </summary>
        /// <param name="bytes">Byte array with MBR sector (512 bytes)</param>
        /// <returns></returns>
        public static List<PartitionInfo> getMBRPartitions(byte[] bytes)
        {
            List<PartitionInfo> result = new List<PartitionInfo>();

            if (bytes.Length == 512)
            {
                var mbr = getMBR(bytes);
                
                for (var idx = 0; idx < 4; idx++)
                {
                    #region Workaround for 4 partitions inside MBR
                    MBRPartitionEntry? mbrPartition = null;

                    // Since CLR does not allow arrays not aligned to x4 - workaround to address particular instances
                    switch (idx)
                    {
                        case 0:
                            mbrPartition = mbr.partition1;
                            break;
                        case 1:
                            mbrPartition = mbr.partition2;
                            break;
                        case 2:
                            mbrPartition = mbr.partition3;
                            break;
                        case 3:
                            mbrPartition = mbr.partition4;
                            break;
                    }
                    #endregion

                    if (mbrPartition != null && mbrPartition.Value.partitionType > 0)
                    {
                        var mbrPart = mbrPartition.Value;

                        #region Get partition type
                        PartitionTypeEnum type = PartitionTypeEnum.Unknown;
                        try
                        {
                            type = (PartitionTypeEnum)mbrPart.partitionType;
                        }
                        catch
                        {
                            // Bypass
                        }
                        #endregion

                        var partition = new PartitionInfo();
                        partition.type = type;
                        partition.offsetStart = mbrPart.lbaFirstSector;
                        partition.length = mbrPart.partitionLengthSectors;

                        result.Add(partition);
                    }
                }
            }

            return result;
        }

        #endregion MBR operations

        #region Format Volume

        public static bool formatFAT32PartitionWin32(String volumeGUID)
        {
            bool result = false;

            var volume = getDiskVolumeByGUIDWMI(volumeGUID);
            if (volume != null)
            {
                var driveName = volume["Name"].ToString();
                FormatFMIFS(driveName);
            }


            return result;
        }

        public static bool formatFAT32PartitionWMI(String volumeGUID)
        {
            bool result = false;

            #region Info log
            var infoMessage = String.Format("Starting FAT32 formatting for partition: {0} ...", volumeGUID);
            Logger.Info(infoMessage);
            #endregion Info log

            var volume = getDiskVolumeByGUIDWMI(volumeGUID);

            // Format volume with
            // uint32 Format(
            //    [in] string FileSystem = "FAT32",
            //    [in] boolean QuickFormat = true,
            //    [in] uint32 ClusterSize = 16384,
            //    [in] string Label = "MiSTer Data",
            //    [in] boolean EnableCompression = false
            // );
            var resultCode = volume.InvokeMethod("Format", new object[] { "FAT32", true, 16384, "MiSTer Data", false });

            #region Analyze result

            var errorMessage = String.Empty;

            // Analyze return codes https://msdn.microsoft.com/en-us/library/aa390432(v=vs.85).aspx
            switch (resultCode)
            {
                case 0:
                    result = true;
                    errorMessage = "OK";
                    break;
                case 1:
                    // Unsupported file system
                    errorMessage = "Unsupported file system";
                    break;
                case 2:
                    // Incompatible media in drive
                    errorMessage = "Incompatible media in drive";
                    break;
                case 3:
                    // Access denied
                    errorMessage = "Access denied";
                    break;
                case 4:
                    // Call cancelled
                    errorMessage = "Call cancelled";
                    break;
                case 5:
                    // Call cancellation request too late
                    errorMessage = "Call cancellation request too late";
                    break;
                case 6:
                    // Volume write protected
                    errorMessage = "Volume write protected";
                    break;
                case 7:
                    // Volume lock failed
                    errorMessage = "Volume lock failed";
                    break;
                case 8:
                    // Unable to quick format
                    errorMessage = "Unable to quick format";
                    break;
                case 9:
                    // Input/Output(I/O) error
                    errorMessage = "I/O error";
                    break;
                case 10:
                    // Invalid volume label
                    errorMessage = "Invalid volume label";
                    break;
                case 11:
                    // No media in drive
                    errorMessage = "No media in drive";
                    break;
                case 12:
                    // Volume is too small
                    errorMessage = "Volume is too small";
                    break;
                case 13:
                    // Volume is too large
                    errorMessage = "Volume is too large";
                    break;
                case 14:
                    // Volume is not mounted
                    errorMessage = "Volume is not mounted";
                    break;
                case 15:
                    // Cluster size is too small
                    errorMessage = "Cluster size is too small";
                    break;
                case 16:
                    // Cluster size is too large
                    errorMessage = "Cluster size is too large";
                    break;
                case 17:
                    // Cluster size is beyond 32 bits
                    errorMessage = "Cluster size is beyond 32 bits";
                    break;
                case 18:
                    // Unknown error
                    errorMessage = "Unknown error";
                    break;
                default:
                    errorMessage = "Unrecognized error code";
                    break;
            }

            if (result)
            {
                Logger.Info(errorMessage);
            }
            else
            {
                Logger.Error(errorMessage);
            }

            #endregion Analyze result

            return result;
        }

        public static bool formatExFATPartitionSystem(String mountPoint)
        {
            bool result = false;

            // Initial validation
            if (String.IsNullOrEmpty(mountPoint) || mountPoint.Contains("C:"))
            {
                Logger.Error("Unable to format disk with letter: '{0}'", mountPoint);
                return result;
            }
            else
            {
                mountPoint = mountPoint.Substring(0, 2);
            }

            string formatExecutable = Path.Combine(Environment.SystemDirectory, "format.com");
            if (!File.Exists(formatExecutable))
            {
                Logger.Error("Executable: {0} not found");
                return result;
            }

            try
            {
                StringBuilder sbError = new StringBuilder();
                StringBuilder sbOutput = new StringBuilder();

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.FileName = formatExecutable;
                startInfo.Arguments = String.Format(DiskFormatPattern, mountPoint);
                process.StartInfo = startInfo;
           
                process.Start();
                process.ErrorDataReceived += (sender, e) => sbError.AppendLine(e.Data);
                process.OutputDataReceived += (sender, e) => sbOutput.AppendLine(e.Data);
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.WaitForExit(20 * 1000); // Set 20 secs. timeout

                // ExitCode throws if the process is hanging
                int resCode = process.ExitCode;
                if (resCode == 0)
                {
                    result = true;

                    Logger.Info("Format finished successfully");
                }
                else
                {
                    Logger.Error("Format error: \n{0}", sbError.ToString());
                }
            }
            catch (InvalidOperationException ioex)
            {
                Logger.Error("Format process hangs. Unable to complete partition format. {0}", ioex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error("Format process failed. {0}", ex.Message);
            }

            return result;
        }

        public static bool formatExFATPartitionWMI(String volumeGUID)
        {
            bool result = false;

            if (!volumeGUID.EndsWith(@"\"))
                volumeGUID += @"\";

            #region Info log
            var infoMessage = String.Format("Starting ExFAT formatting for partition: {0} ...", volumeGUID);
            Logger.Info(infoMessage);
            #endregion Info log
            
            ManagementObject volume = getDiskVolumeByGUIDWMI(volumeGUID);
            if (volume != null)
            {
                try
                {
                    ManagementBaseObject inParams;
                    ManagementBaseObject output;

                    #region Dismount
                    inParams = volume.GetMethodParameters("Dismount");
                    inParams["Force"] = true;
                    inParams["Permanent"] = false;

                    output = volume.InvokeMethod("Dismount", inParams, null);
                    if (output != null)
                    {
                        string message = String.Empty;
                        uint retCode = UInt32.Parse(output["ReturnValue"].ToString());
                        switch (retCode)
                        {
                            case 0:
                                message = "Success";
                                break;
                            case 1:
                                message = "Access Denied";
                                break;
                            case 2:
                                message = "Volume Has Mount Points";
                                break;
                            case 3:
                                message = "Volume Does Not Support The No-Autoremount State";
                                break;
                            case 4:
                                message = "Force Option Required";
                                break;
                            default:
                                message = "Unknown error";
                                break;
                        }

                        Logger.Info("Volume dismount returned: {0}", message);
                    }
                    #endregion Dismount

                    #region Format

                    inParams = volume.GetMethodParameters("Format");
                    inParams["FileSystem"] = "EXFAT";
                    inParams["QuickFormat"] = true;
                    inParams["ClusterSize"] = 32768;
                    inParams["Label"] = "MiSTer Data";
                    inParams["EnableCompression"] = false;

                    int iterations = 10;
                    do
                    {
                        uint retCode = 0;
                        output = volume.InvokeMethod("Format", inParams, null);
                        if (output != null)
                        {
                            string message = String.Empty;
                            retCode = UInt32.Parse(output["ReturnValue"].ToString());
                            switch (retCode)
                            {
                                case 0:
                                    message = "Success";
                                    break;
                                case 1:
                                    message = "Unsupported file system";
                                    break;
                                case 2:
                                    message = "Incompatible media in drive";
                                    break;
                                case 3:
                                    message = "Access denied";
                                    break;
                                case 4:
                                    message = "Call canceled";
                                    break;
                                case 5:
                                    message = "Call cancellation request too late";
                                    break;
                                case 6:
                                    message = "Volume write protected";
                                    break;
                                case 7:
                                    message = "Volume lock failed";
                                    break;
                                case 8:
                                    message = "Unable to quick format";
                                    break;
                                case 9:
                                    message = "Input/Output (I/O) error";
                                    break;
                                case 10:
                                    message = "Invalid volume label";
                                    break;
                                case 11:
                                    message = "No media in drive";
                                    break;
                                case 12:
                                    message = "Volume is too small";
                                    break;
                                case 13:
                                    message = "Volume is too large";
                                    break;
                                case 14:
                                    message = "Volume is not mounted";
                                    break;
                                case 15:
                                    message = "Cluster size is too small";
                                    break;
                                case 16:
                                    message = "Cluster size is too large";
                                    break;
                                case 17:
                                    message = "Cluster size is beyond 32 bits";
                                    break;
                                default:
                                    message = "Unknown error";
                                    break;
                            }

                            if (retCode == 0)
                                result = true;

                            Logger.Info("Volume format returned: {0}", message);
                        }

                        if (retCode == 0)
                        {
                            result = true;
                            break;
                        }

                        Thread.Sleep(200);
                        iterations--;
                    } while (iterations > 0);
                    #endregion Format

                    #region Mount
                    if (result)
                    {
                        inParams = volume.GetMethodParameters("Mount");
                        output = volume.InvokeMethod("Mount", inParams, null);

                        if (output != null)
                        {
                            string message = String.Empty;
                            uint retCode = UInt32.Parse(output["ReturnValue"].ToString());
                            switch (retCode)
                            {
                                case 0:
                                    message = "Success";
                                    break;
                                case 1:
                                    message = "Access Denied";
                                    break;
                                default:
                                    message = "Unknown error";
                                    break;
                            }

                            Logger.Info("Volume mount returned: {0}", message);
                        }
                    }
                    #endregion Mount
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
            }

            return result;
        }

        #endregion Format Volume

        #region Helper methods

        private static ManagementObject getDiskVolumeWMI(String mountPoint)
        {
            if (!mountPoint.EndsWith("\\"))
                mountPoint += "\\";

            var query = String.Format("SELECT * FROM Win32_Volume WHERE Name = '{0}'", mountPoint.Replace("\\", "\\\\"));
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", query);
            var queryCollection = from ManagementObject x in searcher.Get() select x;
            var volume = queryCollection.FirstOrDefault();

            return volume;
        }

        private static ManagementObject getDiskVolumeByGUIDWMI(String volumeGUID)
        {
            if (!volumeGUID.EndsWith("\\"))
                volumeGUID += "\\";

            var query = String.Format("SELECT * FROM Win32_Volume WHERE DeviceID = '{0}'", volumeGUID.Replace("\\", "\\\\"));
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", query);
            var queryCollection = from ManagementObject x in searcher.Get() select x;
            var volume = queryCollection.FirstOrDefault();

            return volume;
        }

        public static uint getDiskSignature()
        {
            Random rand = new Random();

            uint thirtyBits = (uint)rand.Next(1 << 30);
            uint twoBits = (uint)rand.Next(1 << 2);
            uint result = (thirtyBits << 2) | twoBits;

            return result;
        }

        public static byte[] getBytes(object obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] result = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, result, 0, size);
            Marshal.FreeHGlobal(ptr);
            return result;
        }

        public static MBR getMBR(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            MBR mbr = (MBR)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MBR));
            handle.Free();

            return mbr;
        }

        #endregion
    }
}
