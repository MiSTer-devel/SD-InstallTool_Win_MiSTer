using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SDInstallTool.Helpers;
using System.Text.RegularExpressions;
using System.Linq;

namespace SDInstallTool
{
    public static partial class DiskManagement
    {
        #region Win32 Interop methods and structures

        #region Enums
        public enum EMoveMethod : uint
        {
            Begin = 0,
            Current = 1,
            End = 2
        }

        [Flags]
        enum FileShareMode : uint
        {
            FILE_SHARE_READ = 0x00000001,
            FILE_SHARE_WRITE = 0x000000002
        }

        #endregion

        #region Types

        /// <summary>
        /// Represents a disk extent.
        /// </summary>
        /// <remarks>MSDN: http://msdn.microsoft.com/en-us/library/aa363968(v=vs.85).aspx </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct DISK_EXTENT
        {
            public UInt32 DiskNumber;
            public Int64 StartingOffset;
            public Int64 ExtentLength;
        }

        /// <summary>
        /// Represents a physical location on a disk. It is the output buffer for the IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS control code.
        /// </summary>
        /// <remarks>MSDN: http://msdn.microsoft.com/en-us/library/aa365727%28VS.85%29.aspx </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct VOLUME_DISK_EXTENTS
        {
            public UInt32 NumberOfDiskExtents;

            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct)]
            public DISK_EXTENT[] Extents;

            public VOLUME_DISK_EXTENTS(int size)
            {
                NumberOfDiskExtents = 0;
                Extents = new DISK_EXTENT[size];
            }
        }

        /// <summary>
        /// Contains information about a device. This structure is used by the IOCTL_STORAGE_GET_DEVICE_NUMBER control code.
        /// </summary>
        /// <remarks>MSDN: https://msdn.microsoft.com/en-us/library/windows/desktop/bb968801(v=vs.85).aspx</remarks>
        [StructLayout(LayoutKind.Sequential)]
        public class STORAGE_DEVICE_NUMBER
        {
            public int DeviceType;
            public int DeviceNumber;
            public int PartitionNumber;
        };

        /// <summary>
        /// Provides information about the hotplug information of a device
        /// </summary>
        /// <remarks>MSDN: https://msdn.microsoft.com/en-us/library/windows/desktop/aa363466(v=vs.85).aspx</remarks>
        [StructLayout(LayoutKind.Sequential)]
        public class STORAGE_HOTPLUG_INFO
        {
            public int Size = Marshal.SizeOf(typeof(STORAGE_HOTPLUG_INFO));

            [MarshalAs(UnmanagedType.U1)]
            public bool MediaRemovable; // ie. zip, jaz, cdrom, mo, etc. vs hdd

            [MarshalAs(UnmanagedType.U1)]
            public bool MediaHotplug;   // ie. does the device succeed a lock even though its not lockable media?

            [MarshalAs(UnmanagedType.U1)]
            public bool DeviceHotplug;  // ie. 1394, USB, etc.

            [MarshalAs(UnmanagedType.U1)]
            public bool WriteCacheEnableOverride; // This field should not be relied upon because it is no longer used
        };

        #endregion

        #region PInvoke declarations

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFile(
           string lpFileName,
           uint dwDesiredAccess,
           uint dwShareMode,
           IntPtr lpSecurityAttributes,
           uint dwCreationDisposition,
           uint dwFlagsAndAttributes,
           IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint GetFileSizeEx(
            [In] SafeFileHandle hFile,
            [Out] out Int64 lpFileSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlushFileBuffers(SafeFileHandle hFile);

        [DllImport("kernel32.dll", BestFitMapping = true, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadFile(
            [In] SafeFileHandle hFile,
            [Out] byte[] lpBuffer,
            [In] UInt32 nNumberOfBytesToRead,
            [Out] out UInt32 lpNumberOfBytesRead,
            [In, Out] IntPtr lpOverlapped);

        [DllImport("kernel32.dll", BestFitMapping = true, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WriteFile(
            [In] SafeFileHandle hFile,
            [In] byte[] lpBuffer,
            [In] UInt32 nNumberOfBytesToWrite,
            [Out] out UInt32 lpNumberOfBytesWritten,
            [In, Out] IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint SetFilePointer(
            [In] SafeFileHandle hFile,
            [In] UInt32 nDistanceToMove,
            [In, Out] IntPtr lpDistanceToMoveHigh,
            [In] UInt32 dwMoveMethod);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetFilePointerEx(
            [In] SafeFileHandle hFile,
            [In] long liDistanceToMove,
            [Out] out long lpNewFilePointer,
            [In] UInt32 dwMoveMethod);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeviceIoControl(
            [In] SafeFileHandle hDevice,
            [In] UInt32 dwIoControlCode,
            [In] byte[] lpInBuffer,
            [In] UInt32 nInBufferSize,
            [Out] byte[] lpOutBuffer,
            [In] UInt32 nOutBufferSize,
            [Out] out UInt32 lpBytesReturned,
            [In, Out] IntPtr lpOverlapped);

        [DllImport("kernel32", EntryPoint = "DeviceIoControl", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeviceIoControlOut(
            [In] SafeFileHandle hDevice,
            [In] UInt32 dwIoControlCode,
            [In] IntPtr lpInBuffer,
            [In] UInt32 nInBufferSize,
            [Out] IntPtr lpOutBuffer,
            [In] UInt32 nOutBufferSize,
            [Out] out UInt32 lpBytesReturned,
            [In, Out] IntPtr overlapped);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lpszVolumeName"></param>
        /// <param name="cchBufferLength"></param>
        /// <returns>Find Handle to use for FindNextVolumeOperation</returns>
        /// <remarks>Important! Returned find handle needs to be closed only by CloseFindHandler function. Thus SafeFileHandle is not acceptable leading to SEH exception</remarks>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr FindFirstVolume(
            [Out] StringBuilder lpszVolumeName,
            [In] uint cchBufferLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindNextVolume(
            [In] IntPtr hFindVolume,
            [Out] StringBuilder lpszVolumeName,
            [In] uint cchBufferLength);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindVolumeClose(
            [In] IntPtr hFindVolume);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetVolumeNameForVolumeMountPoint(
            [In] string lpszVolumeMountPoint,
            [Out] StringBuilder lpszVolumeName,
            [In] uint cchBufferLength);

        /// <summary>
        /// Retrieves a list of drive letters and mounted folder paths for the specified volume.
        /// </summary>
        /// <param name="lpszVolumeName">A volume GUID path for the volume. A volume GUID path is of the form "\\?\Volume{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}\".</param>
        /// <param name="lpszVolumePathNames">A pointer to a buffer that receives the list of drive letters and mounted folder paths. The list is an array of null-terminated strings terminated by an additional NULL character. If the buffer is not large enough to hold the complete list, the buffer holds as much of the list as possible.</param>
        /// <param name="cchBuferLength">The length of the lpszVolumePathNames buffer, in TCHARs, including all NULL characters.</param>
        /// <param name="lpcchReturnLength">If the call is successful, this parameter is the number of TCHARs copied to the lpszVolumePathNames buffer. Otherwise, this parameter is the size of the buffer required to hold the complete list, in TCHARs.</param>
        /// <returns></returns>
        /// <remarks>MSDN: https://msdn.microsoft.com/en-us/library/windows/desktop/aa364998(v=vs.85).aspx </remarks>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetVolumePathNamesForVolumeName(
            [In] String lpszVolumeName,
            [In, Out] StringBuilder lpszVolumePathNames,
            [In] UInt32 cchBuferLength,
            [Out] out Int32 lpcchReturnLength);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteVolumeMountPoint(string lpszVolumeMountPoint);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetVolumeMountPoint(
            string lpszVolumeMountPoint,
            string lpszVolumeName);

        #endregion PInvoke declarations

        #endregion Win32 Interop methods

        #region Win32 disk access methods

        #region openDiskWin32
        private static SafeFileHandle openDiskWin32(String physicalDiskName)
        {
            SafeFileHandle result = CreateFile(
                physicalDiskName,
                NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
                NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE,
                IntPtr.Zero,
                NativeMethods.OPEN_EXISTING,
                NativeMethods.FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

            return result;
        }

        private static SafeFileHandle openDiskWin32ReadOnly(String physicalDiskName)
        {
            SafeFileHandle result = CreateFile(
                physicalDiskName,
                NativeMethods.GENERIC_READ,
                NativeMethods.FILE_SHARE_VALID_FLAGS,
                IntPtr.Zero,
                NativeMethods.OPEN_EXISTING,
                NativeMethods.FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

            return result;
        }

        private static SafeFileHandle openDiskWinExclusive32(String physicalDiskName)
        {
            SafeFileHandle result = CreateFile(
                physicalDiskName,
                NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
                0,
                IntPtr.Zero,
                NativeMethods.OPEN_EXISTING,
                0,
                IntPtr.Zero);

            return result;
        }

        private static SafeFileHandle openDiskNoBufferingWin32(String physicalDiskName)
        {
            SafeFileHandle result = CreateFile(
                physicalDiskName,
                NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
                NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE,
                IntPtr.Zero,
                NativeMethods.OPEN_EXISTING,
                NativeMethods.FILE_FLAG_NO_BUFFERING,
                IntPtr.Zero);

            return result;
        }

        #endregion openDiskWin32

        #region openVolumeWin32

        private static SafeFileHandle openVolumeWin32(String volumeName)
        {
            SafeFileHandle result = CreateFile(
                volumeName,
                NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
                NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE,
                IntPtr.Zero,
                NativeMethods.OPEN_EXISTING,
                NativeMethods.FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

            return result;
        }

        #endregion openVolumeWin32

        #region openFileWin32

        public static SafeFileHandle openFileWin32(String filePath)
        {
            SafeFileHandle result = CreateFile(
                filePath,
                NativeMethods.GENERIC_READ,
                NativeMethods.FILE_SHARE_READ,
                IntPtr.Zero,
                NativeMethods.OPEN_EXISTING,
                0,
                IntPtr.Zero
                );

            return result;
        }

        #endregion openFileWin32

        #region getFileSizeWin32

        public static long getFileSizeWin32(SafeFileHandle hFile)
        {
            long result = 0;
   
            GetFileSizeEx(hFile, out result);

            return result;
        }

        #endregion getFileSizeWin32

        #region setFilePointerWin32

        public static bool setFilePointerWin32(SafeFileHandle hFile, uint distanceToMove, uint moveMethod)
        {
            bool result = false;

            long newStartOffset = 0;
            result = SetFilePointerEx(hFile, distanceToMove, out newStartOffset, moveMethod);

            return result;
        }

        #endregion setFilePointerWin32

        #region readSectorDataFromHandleWin32

        public static bool readSectorDataFromHandleWin32(SafeFileHandle hHandle, long startSector, long numSectors, int sectorSize, ref byte[] bytes)
        {
            bool result = false;
            uint nNumberOfBytesRead = 0;

            long startOffset = startSector * sectorSize;
            long newStartOffset = 0;

            result = SetFilePointerEx(hHandle, startOffset, out newStartOffset, NativeMethods.FILE_SEEK_BEGIN);
            if (!result) return result;

            uint nNumberOfBytesToRead = (uint)(numSectors * sectorSize);
            if (ReadFile(hHandle, bytes, nNumberOfBytesToRead, out nNumberOfBytesRead, IntPtr.Zero))
            {
                result = true;
            }
            else
            {
                Logger.LogWin32Error();
            }

            return result;
        }

        #endregion readSectorDataFromHandleWin32

        #region writeSectorDataToHandleWin32

        public static bool writeSectorDataToHandleWin32(SafeFileHandle hHandle, byte[] data, long startSector, long numsectors, int sectorSize)
        {
            bool result = false;
            UInt32 nBytesWritten = 0;

            long startOffset = startSector * sectorSize;
            long newStartOffset = 0;

            result = SetFilePointerEx(hHandle, startOffset, out newStartOffset, NativeMethods.FILE_SEEK_BEGIN);
            if (!result) return result;

            uint nNumberOfBytesToWrite = (uint)(numsectors * sectorSize);
            if (WriteFile(hHandle, data, nNumberOfBytesToWrite, out nBytesWritten, IntPtr.Zero))
            {
                result = true;
            }
            else
            {
                Logger.LogWin32Error();
            }

            return result;
        }

        #endregion writeSectorDataToHandleWin32

        #region deleteDriveLayoutWin32

        /// <summary>
        /// Removes the boot signature from the master boot record,
        /// so that the disk will be formatted from sector zero to the end of the disk.
        /// Partition information is no longer stored in sector zero.
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa365158(v=vs.85).aspx
        /// </summary>
        /// <param name="physicalDiskName"></param>
        /// <returns></returns>
        private static bool deleteDriveLayoutWin32(String physicalDiskName)
        {
            bool result = deleteDriveLayoutWin32(openDiskWin32(physicalDiskName));

            return result;
        }

        private static bool deleteDriveLayoutWin32(SafeFileHandle hDisk)
        {
            bool result = false;

            UInt32 nBytesWritten = 0;

            if (hDisk != null && !hDisk.IsInvalid)
            {
                bool res = DeviceIoControl(
                   hDisk,
                   NativeMethods.IOCTL_DISK_DELETE_DRIVE_LAYOUT,
                   null,
                   0,
                   null,
                   0,
                   out nBytesWritten,
                   IntPtr.Zero
                   );

                if (res)
                {
                    result = true;
                }
                else
                {
                    Logger.LogWin32Error();
                }
            }

            return result;
        }

        #endregion deleteDriveLayoutWin32

        #region updateDiskPropertiesWin32

        /// <summary>
        /// Invalidates the cached partition table and re-enumerates the device
        /// </summary>
        /// <param name="hDisk">A handle to the physical disk to be re-enumerated</param>
        /// <returns></returns>
        private static bool updateDiskPropertiesWin32(SafeFileHandle hDisk)
        {
            bool result = false;

            if (hDisk != null && !hDisk.IsInvalid)
            {
                UInt32 nBytesWritten = 0;

                result = DeviceIoControl(
                    hDisk,
                    NativeMethods.IOCTL_DISK_UPDATE_PROPERTIES,
                    null,
                    0,
                    null,
                    0,
                    out nBytesWritten,
                    IntPtr.Zero
                    );
            }

            return result;
        }

        #endregion updateDiskPropertiesWin32

        #region getVolumeNameForVolumeMountPointWin32
        public static String getVolumeNameForVolumeMountPointWin32(String mountPoint)
        {
            String result = String.Empty;

            const int MaxVolumeNameLength = 100;
            StringBuilder sb = new StringBuilder(MaxVolumeNameLength);
            if (!GetVolumeNameForVolumeMountPoint(mountPoint, sb, MaxVolumeNameLength))
            {
                result = sb.ToString();
            }

            return result;
        }
        
        #endregion getVolumeNameForVolumeMountPointWin32

        #region lockVolumeWin32

        private static bool lockVolumeWin32(String volumeName)
        {
            bool result = lockVolumeWin32(openDiskWin32(volumeName));

            return result;
        }

        private static bool lockVolumeWin32(SafeFileHandle hVolume)
        {
            bool result = false;

            if (hVolume != null && !hVolume.IsInvalid)
            {
                UInt32 nBytesWritten = 0;

                result = DeviceIoControl(
                    hVolume,
                    NativeMethods.FSCTL_LOCK_VOLUME,
                    null,
                    0,
                    null,
                    0,
                    out nBytesWritten,
                    IntPtr.Zero);

                if (!result)
                {
                    Logger.LogWin32Error();
                }
            }

            return result;
        }

        #endregion lockVolumeWin32

        #region unlockVolumeWin32

        private static bool unlockVolumeWin32(String volumeName)
        {
            bool result = unlockVolumeWin32(openDiskWin32(volumeName));

            return result;
        }

        private static bool unlockVolumeWin32(SafeFileHandle hVolume)
        {
            bool result = false;

            if (hVolume != null && !hVolume.IsInvalid)
            {
                UInt32 nBytesWritten = 0;

                result = DeviceIoControl(
                    hVolume,
                    NativeMethods.FSCTL_UNLOCK_VOLUME,
                    null,
                    0,
                    null,
                    0,
                    out nBytesWritten,
                    IntPtr.Zero);

                if (!result)
                {
                    Logger.LogWin32Error();
                }
            }

            return result;
        }

        #endregion unlockVolumeWin32

        #region wipeDiskBlockWin32

        private static bool wipeDiskBlockWin32(String physicalDiskName, int sizeMegabytes)
        {
            bool result = false;

            using (SafeFileHandle hDisk = openDiskWin32(physicalDiskName))
            {
                if (lockVolumeWin32(hDisk))
                {
                    if (enableWriteToDiskExtendedAreaWin32(hDisk))
                    {
                        result = wipeDiskBlockWin32(hDisk, sizeMegabytes);
                    }

                    unlockVolumeWin32(hDisk);
                }
            }

            return result;
        }

        private static bool wipeDiskBlockWin32(SafeFileHandle hDisk, int sizeMegabytes)
        {
            bool result = false;

            if (hDisk != null && !hDisk.IsInvalid)
            {
                uint bytesPerSector = 512; // Hardcode until issues found. Then read from drive geometry can be implemented

                var sectorBytes = new byte[bytesPerSector];
                var sectorsToWrite = sizeMegabytes * 1024 / bytesPerSector;

                UInt32 nBytesWritten = 0;
                bool success = false;

                for (int i = 0; i < sectorsToWrite; i++)
                {
                    success = WriteFile(hDisk, sectorBytes, bytesPerSector, out nBytesWritten, IntPtr.Zero);

                    if (!success)
                    {
                        Logger.LogWin32Error();

                        break;
                    }
                }

                result = success;
            }

            return result;
        }

        #endregion wipeDiskBlockWin32

        #region cleanReservedAreasWin32

        private static bool cleanReservedAreasWin32(String physicalDiskName)
        {
            bool result = false;

            Logger.Info("Starting to clean disk reserved areas...");

            var disk = discoverDisk(physicalDiskName);
            prepareDiskForWiping(disk);

            using (SafeFileHandle hDisk = openDiskWin32(physicalDiskName))
            {
                if (dismountVolumeWin32(hDisk))
                {
                    if (lockVolumeWin32(hDisk))
                    {
                        uint bytesPerSector = 512; // Hardcode until issues found. Then read from drive geometry can be implemented

                        // All reserved area + Preloade (U-Boot) partition needs to be wiped, except the very first sector
                        // First disk sector contains MBR
                        var sectorsToWrite = (PARTITION_RESERVED_AREA / bytesPerSector) - 1;
                        var wipeBuffer = new byte[sectorsToWrite * bytesPerSector];

                        setFilePointerWin32(hDisk, bytesPerSector, NativeMethods.FILE_SEEK_BEGIN);

                        UInt32 nBytesWritten = 0;
                        WriteFile(hDisk, wipeBuffer, (uint)wipeBuffer.Length, out nBytesWritten, IntPtr.Zero);

                        unlockVolumeWin32(hDisk);
                    }
                }
            }

            // Step 3. Unlock previously locked volume handles
            unlockDiskAfterWiping(disk);

            Logger.Info("Done");

            return result;
        }

        #endregion cleanReservedAreasWin32

        #region getAllVolumesForDiskWin32
        public static List<String> getAllVolumesForDiskWin32(String physicalDiskName)
        {
            List<String> result = new List<String>();
            SortedDictionary<Int64, String> sortedVolumes = new SortedDictionary<Int64, String>();

            #region Obtain list of all volumes available

            List<String> volumes = new List<String>();
            int diskIndex = int.Parse(Regex.Match(physicalDiskName, @"\d+").Value);
            StringBuilder volumePaths = new StringBuilder(100);

            IntPtr hFindVolume = FindFirstVolume(volumePaths, (uint)volumePaths.Capacity);

            if (hFindVolume != (IntPtr)(-1))
            {
                do
                {
                    volumes.Add(volumePaths.ToString());
                }
                while (FindNextVolume(hFindVolume, volumePaths, (uint)volumePaths.Capacity));

                FindVolumeClose(hFindVolume);
            }
            #endregion

            #region Filter out by disk

            foreach (var volumeGUID in volumes)
            {
                var volumeExtents = getVolumeExtentsForDiskWin32(volumeGUID);

                if (volumeExtents != null)
                {
                    foreach (var extent in volumeExtents)
                    {
                        if (extent.DiskNumber == diskIndex)
                        {
                            sortedVolumes.Add(extent.StartingOffset, volumeGUID);
                        }
                    }
                }
            }

            #endregion

            // Sort by offset ot disk (Ascending)
            result = sortedVolumes.Values.ToList();

            return result;
        }

        #endregion getAllVolumesForDiskWin32

        #region isVolumeMountedWin32

        public static bool isVolumeMountedWin32(String mountPoint)
        {
            SafeFileHandle hVolume = openDiskWin32(mountPoint);

            bool result = isVolumeMountedWin32(hVolume);

            return result;
        }

        public static bool isVolumeMountedWin32(SafeFileHandle hVolume)
        {
            bool result = false;

            if (hVolume != null && !hVolume.IsInvalid)
            {
                UInt32 nBytesWritten = 0;

                result = DeviceIoControl(
                    hVolume,
                    NativeMethods.FSCTL_IS_VOLUME_MOUNTED,
                    null,
                    0,
                    null,
                    0,
                    out nBytesWritten,
                    IntPtr.Zero);

                if (!result)
                {
                    Logger.LogWin32Error();
                }
            }

            return result;
        }
        
        #endregion

        #region dismountVolumeWin32

        private static bool dismountVolumeWin32(String mountPoint)
        {
            bool result = dismountVolumeWin32(openDiskWin32(mountPoint));

            return result;
        }

        private static bool dismountVolumeWin32(SafeFileHandle hVolume)
        {
            bool result = false;

            if (hVolume != null && !hVolume.IsInvalid)
            {
                UInt32 nBytesWritten = 0;

                result = DeviceIoControl(
                    hVolume,
                    NativeMethods.FSCTL_DISMOUNT_VOLUME,
                    null,
                    0,
                    null,
                    0,
                    out nBytesWritten,
                    IntPtr.Zero);

                if (!result)
                {
                    Logger.LogWin32Error();
                }
            }

            return result;
        }

        #endregion dismountVolumeWin32

        #region getVolumeExtentsForDiskWin32

        private static List<DISK_EXTENT> getVolumeExtentsForDiskWin32(String volumeID)
        {
            List<DISK_EXTENT> result = new List<DISK_EXTENT>();

            volumeID = volumeID.TrimEnd('\\');

            using (SafeFileHandle hVolume = openDiskWin32(volumeID))
            {
                if (hVolume != null && !hVolume.IsInvalid)
                {
                    UInt32 nBytesWritten = 0;
                    UInt32 outBufferSize = (UInt32)Marshal.SizeOf(typeof(VOLUME_DISK_EXTENTS));
                    IntPtr outBuffer = Marshal.AllocHGlobal((int)outBufferSize);

                    if (DeviceIoControlOut(
                        hVolume,
                        NativeMethods.IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS,
                        IntPtr.Zero,
                        0,
                        outBuffer,
                        outBufferSize,
                        out nBytesWritten,
                        IntPtr.Zero
                        ))
                    {
                        var volumeExtents = (VOLUME_DISK_EXTENTS)Marshal.PtrToStructure(outBuffer, typeof(VOLUME_DISK_EXTENTS));

                        if (volumeExtents.NumberOfDiskExtents > 0)
                        {
                            foreach (DISK_EXTENT extent in volumeExtents.Extents)
                            {
                                result.Add(extent);
                            }
                        }
                    }

                    Marshal.FreeHGlobal(outBuffer);
                }
                else
                {
                    Logger.LogWin32Error();
                }
            }

            return result;
        }

        #endregion getVolumeExtentsForDiskWin32

        #region flushVolumeWin32

        private static bool flushVolumeWin32(String volumeName)
        {
            bool result = false;

            using (SafeFileHandle hVolume = openDiskWin32(volumeName))
            {
                result = flushVolumeWin32(hVolume);
            }

            return result;
        }

        private static bool flushVolumeWin32(SafeFileHandle hVolume)
        {
            bool result = false;

            if (hVolume != null && !hVolume.IsInvalid)
            {
                FlushFileBuffers(hVolume);

                result = true;
            }

            return result;
        }

        #endregion flushVolumeWin32

        #region mountVolumeWin32

        private static bool mountVolumeWin32(String volumeName, String volumeGUID)
        {
            bool result = false;

            #region Unmount automatically mounted volume

            // Check if volume was automatically remounted by the OS
            StringBuilder volumePaths = new StringBuilder(100);
            Int32 volumePathsLength = 0;
            GetVolumePathNamesForVolumeName(volumeGUID, volumePaths, (uint)volumePaths.Capacity, out volumePathsLength);
            if (volumePathsLength > 0)
            {
                // There can be multiple mount points assigned for the volume - proceed with all
                foreach (var volume in volumePaths.ToString().Split('\0'))
                {
                    if (volume.Length == 3)
                    {
                        deleteVolumeMountWin32(volume);
                    }
                }
            }

            #endregion

            result = setVolumeMountPointWin32(volumeName, volumeGUID);
            if (!result)
            {
                var resCode = Marshal.GetLastWin32Error();

                // Happens if OS re-mounts the volume automatically
                if (resCode == NativeMethods.ERROR_DIR_NOT_EMPTY)
                {

                }
            }

            return result;
        }

        #endregion mountVolumeWin32

        #region setAutoMountWin32

        private static bool setAutoMountWin32(bool enable)
        {
            bool result = false;

            UInt32 nBytesWritten = 0;

            SafeFileHandle hVolume = CreateFile(
                NativeMethods.MOUNTMGR_DOS_DEVICE_NAME,
                NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
                NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE,
                IntPtr.Zero,
                NativeMethods.OPEN_EXISTING,
                NativeMethods.FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

            if (!hVolume.IsInvalid)
            {
                // DeviceIoControl expects BOOL type (aka int, 4 bytes)
                byte[] apiBOOLEnabled = new byte[4];
                apiBOOLEnabled[0] = Convert.ToByte(enable);

                result = DeviceIoControl(
                    hVolume,
                    NativeMethods.IOCTL_MOUNTMGR_SET_AUTO_MOUNT,
                    apiBOOLEnabled,
                    (uint)apiBOOLEnabled.Length,
                    null,
                    0,
                    out nBytesWritten,
                    IntPtr.Zero);
            }

            return result;
        }

        #endregion setAutoMountWin32

        #region getAutoMountWin32

        private static bool getAutoMountWin32()
        {
            bool result = false;

            UInt32 nBytesWritten = 0;

            SafeFileHandle hDeviceManager = CreateFile(
                NativeMethods.MOUNTMGR_DOS_DEVICE_NAME,
                NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
                NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE,
                IntPtr.Zero,
                NativeMethods.OPEN_EXISTING,
                NativeMethods.FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

            if (!hDeviceManager.IsInvalid)
            {
                // DeviceIoControl expects BOOL type (aka int, 4 bytes)
                byte[] apiBOOLEnabled = new byte[4];

                result = DeviceIoControl(
                    hDeviceManager,
                    NativeMethods.IOCTL_MOUNTMGR_QUERY_AUTO_MOUNT,
                    null,
                    0,
                    apiBOOLEnabled,
                    (uint)apiBOOLEnabled.Length,
                    out nBytesWritten,
                    IntPtr.Zero);

                result = Convert.ToBoolean(apiBOOLEnabled[0]);

                hDeviceManager.Dispose();
            }

            return result;
        }

        #endregion getAutoMountWin32

        #region deleteVolumeMountWin32

        private static bool deleteVolumeMountWin32(String volumeName)
        {
            bool result = false;

            result = DeleteVolumeMountPoint(volumeName);

            return result;
        }

        #endregion deleteVolumeMountWin32

        #region setVolumeMountPointWin32

        private static bool setVolumeMountPointWin32(String mountPointName, String volumeGUID)
        {
            bool result = false;

            result = SetVolumeMountPoint(mountPointName, volumeGUID);

            return result;
        }

        #endregion setVolumeMountPointWin32

        #endregion

        #region Unused Win32

        #region enableWriteToDiskExtendedAreaWin32

        /// <summary>
        /// Problem: Notifies only volume filesystem, so useless to write into MBR / reserved disk space
        /// </summary>
        /// <param name="logicalVolumeName"></param>
        /// <returns></returns>
        private static bool enableWriteToDiskExtendedAreaWin32(String logicalVolumeName)
        {
            bool result = enableWriteToDiskExtendedAreaWin32(openDiskWin32(logicalVolumeName));

            return result;
        }

        /// <summary>
        /// Signals the file system driver not to perform any I/O boundary checks on partition read or write calls.
        /// Instead, boundary checks are performed by the device driver.
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa364556(v=vs.85).aspx
        /// </summary>
        /// <param name="hDisk"></param>
        /// <returns></returns>
        private static bool enableWriteToDiskExtendedAreaWin32(SafeFileHandle hDisk)
        {
            bool result = false;

            UInt32 nBytesWritten = 0;

            if (hDisk != null && !hDisk.IsInvalid)
            {
                var res = DeviceIoControl(
                    hDisk,
                    NativeMethods.FSCTL_ALLOW_EXTENDED_DASD_IO,
                    null,
                    0,
                    null,
                    0,
                    out nBytesWritten,
                    IntPtr.Zero
                    );

                if (res)
                {
                    result = true;
                }
                else
                {
                    Logger.LogWin32Error();
                }
            }

            return result;
        }
        #endregion enableWriteToDiskExtendedAreaWin32

        #endregion
    }
}
