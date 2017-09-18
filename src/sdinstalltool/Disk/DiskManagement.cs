using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using Microsoft.Win32.SafeHandles;
using SDInstallTool.Helpers;
using System.Text;
using System.IO;
using System.Security;

namespace SDInstallTool
{
    public static partial class DiskManagement
    {
        #region Constants

        // Minimal supported SD card size is ~2GB (or 1.86 GiB)
        private static readonly ulong SD_CARD_MIN_SIZE = 2 * 1000 * 1000;

        // Raw disk area to be filled with zeroes during wipe operation (in Megabytes)
        private static readonly int WIPE_DISK_AREA_MEGABYTES = 4;

        // Reserved aread at the disk begin (in bytes)
        private static readonly ulong PARTITION_RESERVED_AREA = 1024 * 1024;

        // Altera SoC preloader partition size (in bytes)
        private static readonly ulong PARTITION_0_SIZE = 1024 * 1024;

        // Linux partition size (in bytes)
        private static readonly ulong PARTITION_1_SIZE = 500 * 1024 * 1024;

        // Write buffer size (in sectors)
        private static readonly int WRITE_BUFFER_SECTORS = 2 * 1024;

        #endregion

        #region Types

        public enum DiskType
        {
            Unknown,
            Fixed,
            Removable
        }

        public struct DiskDescriptor
        {
            public String displayName;
            public String physicalName;
            public String description;
            public ulong sizeBytes;
            public DiskType type;

            public uint bytesPerSector;
            public ulong totalSectors;
        }

        public struct PartitionDescriptor
        {
            // Provided fields
            public ulong offsetSec;
            public ulong lenSec;

            public uint sectorSize;

            // Calculated fields
            public ulong endSec;

            public ulong offsetBytes;
            public ulong lenBytes;
            public ulong endBytes;

            public PartitionDescriptor(ulong offsetSec, ulong lenSec, uint sectorSize = 512)
            {
                this.offsetSec = offsetSec;
                this.lenSec = lenSec;

                this.sectorSize = sectorSize;

                this.endSec = offsetSec + lenSec;

                this.offsetBytes = offsetSec * sectorSize;
                this.lenBytes = lenSec * sectorSize;
                this.endBytes = this.endSec * sectorSize;
            }
        }


        public class Disk
        {
            public String physicalName;
            public int bytesPerSector;

            public List<DiskPartition> partitions = new List<DiskPartition>();
            public List<DiskVolume> volumes = new List<DiskVolume>();
        }

        public class DiskPartition
        {
            public String partitionID;
            public String mountPoint;

            public uint offsetSec;
            public uint sizeSec;
        }

        public struct DiskVolume
        {
            public String GUID;
            public String mountPoint;

            public UInt64 offsetSec;
            public UInt64 sizeSec;
            public SafeFileHandle hVolume;
        }

        #endregion

        #region Business logic methods

        //[HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static bool wipeDisk(String physicalDiskName)
        {
            bool result = false;

            // Step 0: Get disk structure
            var disk = discoverDisk(physicalDiskName);

            #region Analyze mount points vs partitions

            bool diskHasNonMountedPartitions = false;

            if (disk.partitions.Count > 0 && disk.partitions.Count > disk.volumes.Count)
            {
                diskHasNonMountedPartitions = true;
            }
            #endregion

            // Step 1: Unmount and lock all logical volumes located on a disk
            prepareDiskForWiping(disk);

            // Step 2. Open physical disk
            using (SafeFileHandle hDisk = openDiskWin32(physicalDiskName))
            {
                // Step 4: Dismount the whole disk
                if (dismountVolumeWin32(hDisk))
                {
                    // Step 5: Allow extended are operations (no I/O boundaries check)
                    //if (enableWriteToDiskExtendedAreaWin32(hDisk))
                    {
                        // Step 6: Delete drive layout (MBR sector wiped)
                        if (deleteDriveLayoutWin32(hDisk))
                        {
                            setProgress(50);

                            // Step 7: Wipe the whole reserved area (reserved - 1MB, preloader partition - 1MB, Start of Linux Ext4 - 2MB)
                            if (!diskHasNonMountedPartitions)
                                result = wipeDiskBlockWin32(hDisk, WIPE_DISK_AREA_MEGABYTES);

                            // Step 8: Write MBR signature
                            result = writeEmptyMBR(hDisk);
                            setProgress(75);

                            // Step 8: Re-enumerate disk in OS
                            updateDiskPropertiesWin32(hDisk);

                            setProgress(100);
                        }
                    }
                }
            }

            // Step 3. Unlock previously locked volume handles
            unlockDiskAfterWiping(disk);

            return result;
        }

        public static bool fullInstall(String physicalDiskName)
        {
            bool result = false;

            // Step 1: Verify that content will fit on SD card
            if (verifyDiskSize(physicalDiskName))
            {
                // Step 2: Disable automount for new volumes (otherwise newly created partitions will be immediately mounted)
                bool autoMountState = getAutoMountWin32();
                if (autoMountState)
                    setAutoMountWin32(false);

                // Step 3: Perform full wipe (MBR + first few megabytes)
                if (wipeDisk(physicalDiskName))
                {
                    setProgress(5);

                    // Step 4: Calculate partition sizes based on SD card capacity
                    // Partition 0: Fixed size (Altera SoC preloader)
                    // Partition 1: Fixed size (Linux Ext4)
                    // Partition 2: Flexible size (ExFAT)
                    var partitions = calculatePartitions(physicalDiskName);

                    // Step 5: Write partition information into MBR
                    if (createPartitions(physicalDiskName, partitions))
                    {
                        setProgress(10);

                        // Re-read disk structure according new partitioning
                        var disk = discoverDisk(physicalDiskName);

                        if (disk != null && disk.partitions.Count >= 3)
                        {
                            var bytesPerSector = disk.bytesPerSector;

                            var fatVolumeGUID = String.Empty;
                            if (disk.volumes.Count == 0)
                            {
                                // WMI was unable to provide information about volume
                                // Let's retrieve from WinAPI
                                var volumes = getAllVolumesForDiskWin32(physicalDiskName);
                                if (volumes.Count > 0)
                                {
                                    // TODO: Probably it's necessary to match volume by size / FS type
                                    fatVolumeGUID = volumes[0];
                                }
                            }
                            if (disk.volumes.Count == 1)
                            {
                                // Windows 7 sees only FAT/FAT32/ExFAT/NTFS volumes
                                fatVolumeGUID = disk.volumes[0].GUID;
                            }
                            else if (disk.volumes.Count >= 3)
                            {
                                // Windows 10 sees all volumes intependently on partition type
                                fatVolumeGUID = disk.volumes[2].GUID;
                            }

                            var preloaderPartition = disk.partitions[0];
                            var preloaderPartitionOffsetSector = preloaderPartition.offsetSec;
                            var preloaderPartitionSizeSectors = preloaderPartition.sizeSec;

                            var linuxPartition = disk.partitions[1];
                            var linuxPartitionOffsetSector = linuxPartition.offsetSec;
                            var linuxPartitionSizeSectors = linuxPartition.sizeSec;

                            #region Extra sanity check
                            var referencePreloaderPartitionSizeSectors = (uint)(PARTITION_0_SIZE / (ulong)bytesPerSector);
                            var referenceLinuxPartitionSizeSectors = (uint)(PARTITION_1_SIZE / (ulong)bytesPerSector);

                            if (preloaderPartitionSizeSectors > referencePreloaderPartitionSizeSectors ||
                                linuxPartitionSizeSectors > referenceLinuxPartitionSizeSectors)
                            {
                                Logger.Error("Partition size(s) are smaller than required. Stopping further processing");
                                return result;
                            }

                            #endregion Extra sanity check

                            // Unmount all points related to the disk
                            // OS tries to mount Linux and FAT partitions immediately
                            foreach (var volume in disk.volumes)
                            {
                                dismountVolume(volume.GUID);
                            }
                            setProgress(15);

                            // Step 6: Initiate images copy into each fixed-size partition
                            // Step 6.1: Write preloader partition from image file
                            writePreloaderPartitionFromFile(
                                physicalDiskName,
                                preloaderPartitionOffsetSector,
                                preloaderPartitionSizeSectors,
                                bytesPerSector,
                                true,
                                15,
                                20);

                            // Step 6.2: Write Linux partition from image file
                            writeLinuxPartitionFromFile(
                                physicalDiskName,
                                linuxPartitionOffsetSector,
                                linuxPartitionSizeSectors,
                                bytesPerSector,
                                false,
                                20,
                                90);
                            setProgress(90);

                            // Step 6.3: Format ExFAT partition programmatically
                            String mountPoint = formatDataPartition(fatVolumeGUID);
                            setProgress(98);

                            if (!String.IsNullOrEmpty(mountPoint))
                            {
                                // We treat as success only when partition successfully formatted and has letter assigned
                                result = true;

                                // Step 7 (Optional): Unpack mister data files to Data partition
                                if (ImageManager.checkMisterPackage())
                                {
                                    ImageManager.unpackMisterPackage(mountPoint);
                                }
                            }

                            setProgress(100);
                        }
                    }
                }

                // Step 7: Restore automount state
                if (autoMountState)
                    setAutoMountWin32(true);
            }
            else
            {
                // Disk size is insufficient to fit images
            }

            return result;
        }

        public static bool updateLinux(String physicalDiskName)
        {
            bool result = false;

            // Read disk structure
            var disk = discoverDisk(physicalDiskName);
            var bytesPerSector = disk.bytesPerSector;

            if (disk != null && disk.partitions.Count >= 3)
            {
                // Partitions in disk descriptor already sorted by start offset
                var preloaderPartition = disk.partitions[0];
                var preloaderPartitionOffsetSectors = preloaderPartition.offsetSec;
                var preloaderPartitionSizeSectors = preloaderPartition.sizeSec;

                var linuxPartition = disk.partitions[1];
                var linuxPartitionOffsetSectors = linuxPartition.offsetSec;
                var linuxPartitionSizeSectors = linuxPartition.sizeSec;

                cleanReservedAreas(physicalDiskName);
                writePreloaderPartitionFromFile(physicalDiskName, preloaderPartitionOffsetSectors, preloaderPartitionSizeSectors, bytesPerSector, true, 0, 10);
                writeLinuxPartitionFromFile(physicalDiskName, linuxPartitionOffsetSectors, linuxPartitionSizeSectors, bytesPerSector, true, 10, 100);
                setProgress(100);

                result = true;
            }

            return result;
        }

        #endregion Business logic methods

        #region Logic support methods


        #region wipeDisk support
        //[HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static bool prepareDiskForWiping(Disk disk)
        {
            bool result = false;

            if (disk == null)
                return result;

            try
            {
                for (var idx = 0; idx < disk.volumes.Count; idx++)
                {
                    var volume = disk.volumes[idx];
                    SafeFileHandle hVolume = null;

                    if (!String.IsNullOrEmpty(volume.mountPoint))
                    {
                        // Volume is mounted, so need to dismount first
                        hVolume = openDiskWin32(getVolumeName(volume.mountPoint));

                        if (isVolumeMountedWin32(hVolume))
                        {
                            dismountVolumeWin32(hVolume);
                        }

                        lockVolumeWin32(hVolume);
                    }
                    else
                    {
                        // Volume has no mount point, so just lock it
                        hVolume = openDiskWin32(volume.GUID);
                        lockVolumeWin32(hVolume);
                    }

                    volume.hVolume = hVolume;

                    // Actualize object in a collection
                    disk.volumes[idx] = volume;
                }
            }
            catch
            {
                Logger.Error("Error during preparing disk {0} for wiping", disk.physicalName);
            }

            return result;
        }

        private static void unlockDiskAfterWiping(Disk disk)
        {
            try
            {
                for (var idx = 0; idx < disk.volumes.Count; idx++)
                {
                    var volume = disk.volumes[idx];

                    if (volume.hVolume != null && !volume.hVolume.IsInvalid)
                    {
                        unlockVolumeWin32(volume.hVolume);
                        volume.hVolume.Dispose();
                    }
                    else
                    {
                        using (SafeFileHandle hVolume = openDiskWin32(volume.GUID))
                        {
                            if (hVolume != null && !hVolume.IsInvalid)
                            {
                                unlockVolumeWin32(hVolume);
                            }
                        }
                    }

                    volume.hVolume = null;

                    // Actualize object in a collection
                    disk.volumes[idx] = volume;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        private static bool writeEmptyMBR(SafeFileHandle hDisk)
        {
            bool result = false;

            var mbr = new MBR(PartitionManagement.getDiskSignature());
            var mbrBytes = PartitionManagement.getBytes(mbr);

            if (writeMBR(hDisk, mbrBytes))
            {
                result = true;
            }

            return result;
        }

        #endregion wipeDisk support

        #region fullInstall support

        private static bool verifyDiskSize(String physicalDiskName)
        {
            bool result = false;

            ulong diskSize = getDiskSize(physicalDiskName);

            if (diskSize >= SD_CARD_MIN_SIZE)
            {
                result = true;
            }

            return result;
        }

        private static bool createPartitions(String physicalDiskName, List<PartitionInfo> partitions)
        {
            bool result = false;

            if (partitions != null && partitions.Count > 0)
            {
                byte[] mbrBytes = PartitionManagement.createMBRSector(partitions);

                using (SafeFileHandle hDisk = openDiskWin32(physicalDiskName))
                {
                    if (!hDisk.IsInvalid)
                    {
                        if (writeMBR(hDisk, mbrBytes))
                        {
                            result = true;

                            // Re-enumerate disk in OS
                            updateDiskPropertiesWin32(hDisk);
                        }
                    }
                }
            }

            return result;
        }

        private static List<PartitionInfo> calculatePartitions(String physicalDiskName)
        {
            List<PartitionInfo> result = new List<PartitionInfo>();

            var disk = geDiskDriveWMI(physicalDiskName);
            if (disk != null)
            {
                var diskSize = getDiskSize(disk);
                var diskSizeSectors = getDiskSizeSec(disk);
                var sectorSize = getDiskBytesPerSector(disk);

                result = calculatePartitions(diskSizeSectors, sectorSize);
            }

            return result;
        }

        private static List<PartitionInfo> calculatePartitions(ulong diskSizeSectors, uint sectorSize)
        {
            List<PartitionInfo> result = new List<PartitionInfo>();

            #region Partition sizes in sectors calculations
            uint reservedSpaceSectors = Convert.ToUInt32(PARTITION_RESERVED_AREA / sectorSize);
            uint preloaderPartitionSectors = Convert.ToUInt32(PARTITION_0_SIZE / sectorSize);
            uint linuxPartitionSectors = Convert.ToUInt32(PARTITION_1_SIZE / sectorSize);
            uint usedSizeSectors = reservedSpaceSectors + preloaderPartitionSectors + linuxPartitionSectors;

            if (usedSizeSectors >= diskSizeSectors)
            {
                // No room for ExFAT data partition
                return result;
            }

            uint dataPartitionSectors = Convert.ToUInt32(diskSizeSectors - usedSizeSectors);

            #endregion

            #region Create PartitionInfo structures

            PartitionInfo preloaderPartition = new PartitionInfo();
            preloaderPartition.type = PartitionTypeEnum.SoCPreloader;
            preloaderPartition.offsetStart = reservedSpaceSectors;
            preloaderPartition.length = preloaderPartitionSectors;

            PartitionInfo linuxPartition = new PartitionInfo();
            linuxPartition.type = PartitionTypeEnum.Linux;
            linuxPartition.offsetStart = preloaderPartition.offsetStart + preloaderPartition.length;
            linuxPartition.length = linuxPartitionSectors;

            PartitionInfo dataPartition = new PartitionInfo();
            dataPartition.type = PartitionTypeEnum.ExFAT;
            dataPartition.offsetStart = linuxPartition.offsetStart + linuxPartition.length;
            dataPartition.length = dataPartitionSectors;

            #endregion

            #region Pack the result in proper order

            // Data partition should go into MBR first. Otherwise Windows OS unable to mount it
            result.Add(dataPartition);

            // Linux goes into second MBR partition record
            result.Add(linuxPartition);

            // SoC preloader partition goes into third MBR partition record
            result.Add(preloaderPartition);

            #endregion

            return result;
        }

        private static bool cleanReservedAreas(String physicalDiskName)
        {
            bool result = cleanReservedAreasWin32(physicalDiskName);

            return result;
        }

        private static bool writePreloaderPartitionFromFile(
            String physicalDiskName,
            uint startOffset,
            uint maxSectors,
            int sectorSize,
            bool wipeRemainder = false,
            int startProgress = 0,
            int finishProgress = 0)
        {
            var imageFilePath = ImageManager.UbootPartitionFileName;
            bool result = writeVolumeFromImage(
                physicalDiskName,
                startOffset,
                imageFilePath,
                maxSectors,
                sectorSize,
                wipeRemainder,
                startProgress,
                finishProgress);

            return result;
        }

        private static bool writeLinuxPartitionFromFile(
            String physicalDiskName,
            uint startOffset,
            uint maxSectors,
            int sectorSize,
            bool wipeRemainder = false,
            int startProgress = 0,
            int finishProgress = 0)
        {
            var imageFilePath = ImageManager.LinuxPartitionFileName;
            bool result = writeVolumeFromImage(
                physicalDiskName,
                startOffset,
                imageFilePath,
                maxSectors,
                sectorSize,
                wipeRemainder,
                startProgress,
                finishProgress);

            return result;
        }

        private static String formatDataPartition(String volumeGUID)
        {
            String result = String.Empty;

            String mountPoint = mountVolume(volumeGUID);

            if (!String.IsNullOrEmpty(mountPoint))
            {
                bool res = false;

                #region WMI formatting (disabled)
                /*
                #region Info Log
                Logger.Info("Trying to format data partition using WMI...");
                #endregion

                result = PartitionManagement.formatExFATPartitionWMI(volumeGUID);

                #region Info Log
                if (result)
                {
                    Logger.Info("OK");
                }
                else
                {
                    Logger.Info("Failed");
                }
                #endregion
                */
                #endregion

                if (!res)
                {
                    #region Info Log
                    Logger.Info("Trying to format data partition using VDS...");
                    #endregion

                    res = PartitionManagement.formatExFATPartitionVDS(volumeGUID);
                    if (res)
                    {
                        result = mountPoint;
                    }

                    #region Info Log
                    if (res)
                    {
                        Logger.Info("OK");
                    }
                    else
                    {
                        Logger.Info("Failed");
                    }
                    #endregion
                }

                // Flush all pending changes to the volume
                flushVolumeWin32(volumeGUID);
            }

            return result;
        }

        /// <summary>
        /// Mount physical Volume specified by Volume GUID using first available drive letter
        /// (Note: if volume was already mounted with another letter - it will be unmounted)
        /// </summary>
        /// <param name="volumeGUID"></param>
        /// <returns></returns>
        private static String mountVolume(String volumeGUID)
        {
            String result = String.Empty;

            // Flush all pending changes to the volume
            flushVolumeWin32(volumeGUID);

            // Get next available letter for the volume
            var letter = getAvailableMountPoint();
            if (!String.IsNullOrEmpty(letter))
            {
                var mountPoint = String.Format("{0}:\\", letter);
                if (!volumeGUID.EndsWith("\\"))
                    volumeGUID += "\\";

                // Mount with new drive letter unmounting any auto-assignments
                if (mountVolumeWin32(mountPoint, volumeGUID))
                {
                    result = mountPoint;
                }

                unlockVolumeWin32(mountPoint);
            }

            return result;
        }

        private static bool dismountVolume(String volumeGUID)
        {
            bool result = false;

            using (SafeFileHandle hVolume = openDiskWin32(volumeGUID))
            {
                // Flush all pending changes to the volume
                flushVolumeWin32(hVolume);

                dismountVolumeWin32(hVolume);
            }

            #region Unmount automatically mounted volume

            if (!volumeGUID.EndsWith("\\"))
                volumeGUID += "\\";

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

            return result;
        }

        #endregion fullInstall support

        #region updateLinux support

        private static bool isLinuxPartitionValid(String physicalDiskName)
        {
            bool result = false;

            SafeFileHandle hDisk = openDiskWin32(physicalDiskName);


            return result;
        }

        #endregion updateLinux support

        #endregion Logic support methods

        #region Disk information methods
        public static List<String> getPhysicalDeviceList()
        {
            var result = new List<String>();

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDrive");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    var mediaType = queryObj["MediaType"];
                    var physicalDeviceName = queryObj["DeviceID"].ToString();

                    result.Add(physicalDeviceName);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return result;
        }

        public static List<DiskDescriptor> getRemovableDisks()
        {
            var result = new List<DiskDescriptor>();

            var physicalDiskNames = DiskManagement.getPhysicalDeviceList();

            if (physicalDiskNames != null && physicalDiskNames.Count > 0)
            {
                foreach (String diskName in physicalDiskNames)
                {
                    var wmiDisk = geDiskDriveWMI(diskName);
                    uint diskIdx = Convert.ToUInt32(wmiDisk["Index"]);
                    bool isRemovable = isDiskRemovableWMI(wmiDisk);

                    if (isRemovable)
                    {
                        DiskDescriptor diskDescriptor = new DiskDescriptor();
                        diskDescriptor.displayName = String.Format("Disk {0}", diskIdx);
                        diskDescriptor.description = getDiskDescriptionWMI(wmiDisk);
                        diskDescriptor.physicalName = diskName;
                        diskDescriptor.sizeBytes = getDiskSize(wmiDisk);
                        diskDescriptor.type = DiskType.Removable;
                        result.Add(diskDescriptor);
                    }
                }
            }

            return result;
        }

        public static List<ManagementObject> getDiskPartitions(String physicalDiskName)
        {
            var result = new List<ManagementObject>();

            var query = String.Format("ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{0}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition", physicalDiskName);
            ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("root\\CIMV2", query);
            foreach (ManagementObject partition in searcher2.Get())
            {
                result.Add(partition);
            }

            return result;
        }

        public static List<ManagementObject> getDiskVolumesFromPartitions(String physicalDiskName)
        {
            var partitions = getDiskPartitions(physicalDiskName);
            var volumes = new List<ManagementObject>();

            if (partitions != null && partitions.Count > 0)
            {
                foreach (var partition in partitions)
                {
                    var partitionID = partition["DeviceID"].ToString();
                    var volume = getPartitionLogicDiskWMI(partitionID);

                    if (volume != null)
                        volumes.Add(volume);
                }
            }

            return volumes;
        }

        public static List<DiskVolume> getDiskVolumes(String physicalDiskName)
        {
            var result = new List<DiskVolume>();

            try
            {
                var diskDescriptor = getDiskDescriptor(physicalDiskName);
                var disk = geDiskDriveWMI(physicalDiskName);
                var diskIndex = Convert.ToInt32(disk["Index"]);
                var bytesPerSector = diskDescriptor.bytesPerSector;


                var volumeGUIDs = getAllVolumesGUIDNamesWMI();

                foreach (String guid in volumeGUIDs)
                {
                    var volumeExtents = getVolumeExtentsForDiskWin32(guid);

                    if (volumeExtents != null)
                    {
                        foreach (var extent in volumeExtents)
                        {
                            if (extent.DiskNumber == diskIndex)
                            {
                                var offsetSec = (ulong)(extent.StartingOffset / bytesPerSector);
                                var sizeSec = (ulong)(extent.ExtentLength / bytesPerSector);

                                DiskVolume volume = new DiskVolume();
                                volume.GUID = guid;
                                volume.offsetSec = offsetSec;
                                volume.sizeSec = sizeSec;

                                result.Add(volume);
                            }
                        }
                    }
                }

                // Sort volumes by their offset on disk (Ascending)
                result = result.OrderBy(o => o.offsetSec).ToList();

                // If everything went well, we get all 3 partitions
                // 1) UBoot preloader
                // 2) Linux
                // 3) FAT32/exFAT
            }
            catch
            {
                Logger.Error("Unable to get disk volumes: {0}", physicalDiskName);
            }

            return result;
        }

        public static DiskDescriptor getDiskDescriptor(String physicalDiskName)
        {
            DiskDescriptor result = new DiskDescriptor();

            try
            {
                var wmiDisk = geDiskDriveWMI(physicalDiskName);
                uint diskIdx = Convert.ToUInt32(wmiDisk["Index"]);

                result.displayName = String.Format("Disk {0}", diskIdx);
                result.description = getDiskDescriptionWMI(wmiDisk);
                result.physicalName = physicalDiskName;
                result.sizeBytes = getDiskSize(wmiDisk);
                result.type = DiskType.Removable;

                result.bytesPerSector = getDiskBytesPerSector(wmiDisk);
                result.totalSectors = Convert.ToUInt64(wmiDisk["TotalSectors"]);
            }
            catch
            {
                Logger.Error("Unable to get disk descriptor: {0}", physicalDiskName);
            }

            return result;
        }

        #region getDiskSize

        public static ulong getDiskSize(String physicalDiskName)
        {
            ulong result = getDiskSize(geDiskDriveWMI(physicalDiskName));

            return result;
        }

        public static ulong getDiskSize(ManagementObject queryObj)
        {
            ulong result = 0;

            try
            {
                var size = Convert.ToUInt64(queryObj["Size"]);

                result = size;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return result;
        }

        #endregion getDiskSize

        public static ulong getDiskSizeSec(ManagementObject queryObj)
        {
            ulong result = 0;

            try
            {
                var size = Convert.ToUInt64(queryObj["TotalSectors"]);

                result = size;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return result;
        }

        public static uint getDiskBytesPerSector(ManagementObject queryObj)
        {
            uint result = 512;

            try
            {
                var size = Convert.ToUInt32(queryObj["BytesPerSector"]);

                result = size;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return result;
        }

        #endregion

        #region Raw disk write methods

        public static bool wipeServiceArea(String physicalDiskName, uint sizeMegabytes)
        {
            var diskDescriptor = getDiskDescriptor(physicalDiskName);

            return wipeServiceArea(diskDescriptor, sizeMegabytes);
        }

        public static bool wipeServiceArea(DiskDescriptor disk, uint sizeMegabytes)
        {
            bool result = false;

            var physicalDiskName = disk.physicalName;
            var sectorBytes = new byte[disk.bytesPerSector];
            var sectorsToWrite = sizeMegabytes * 1024 / disk.bytesPerSector;

            // Enable raw disk write operations
            var hDisk = openDiskNoBufferingWin32(physicalDiskName);
            if (hDisk != null && !hDisk.IsInvalid)
            {
                bool success = false;
                UInt32 nBytesWritten = 0;

                for (int i = 0; i < sectorsToWrite; i++)
                {
                    success = WriteFile(hDisk, sectorBytes, disk.bytesPerSector, out nBytesWritten, IntPtr.Zero);

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

        #region readMBR

        public static byte[] readMBR(String physicalDiskName)
        {
            byte[] result = new byte[0];

            using (SafeFileHandle hDisk = openDiskWin32ReadOnly(physicalDiskName))
            {
                result = readMBR(hDisk);
            }

            return result;
        }

        public static byte[] readMBR(SafeFileHandle hDisk)
        {
            byte[] result = new byte[0];

            if (hDisk != null && !hDisk.IsInvalid)
            {
                if (setFilePointerWin32(hDisk, 0, NativeMethods.FILE_SEEK_BEGIN))
                {
                    uint nNumberOfBytesRead = 0;

                    var buffer = new byte[512];
                    if (ReadFile(hDisk, buffer, Convert.ToUInt32(buffer.Length), out nNumberOfBytesRead, IntPtr.Zero))
                    {
                        result = buffer;
                    }
                }
            }

            return result;
        }

        #endregion readMBR

        #region writeMBR

        public static bool writeMBR(String physicalDiskName, byte[] bytes)
        {
            bool result = writeMBR(openDiskWin32(physicalDiskName), bytes);

            return result;
        }

        public static bool writeMBR(SafeFileHandle hDisk, byte[] bytes)
        {
            bool result = false;

            if (hDisk != null && !hDisk.IsInvalid)
            {
                if (setFilePointerWin32(hDisk, 0, NativeMethods.FILE_SEEK_BEGIN))
                {
                    UInt32 nBytesWritten = 0;

                    result = WriteFile(hDisk, bytes, Convert.ToUInt32(bytes.Length), out nBytesWritten, default(IntPtr));

                    if (!result)
                    {
                        Logger.LogWin32Error();
                    }
                }
            }

            return result;
        }

        #endregion writeMBR

        #endregion

        #region Helper methods

        public static Disk discoverDisk(String physicalDiskName)
        {
            uint bytesPerSector = getDiskDescriptor(physicalDiskName).bytesPerSector;

            Disk result = new Disk();
            result.physicalName = physicalDiskName;
            result.bytesPerSector = (int)bytesPerSector;

            // Obtain all volumes that belongs to physical disk (volume != partition)
            // Partition is a record in MBR
            // Volume - entity served by certain FileSystem(s) based on partition information
            result.volumes = getDiskVolumes(physicalDiskName);

            var partitions = getDiskPartitions(physicalDiskName);
            foreach (var partition in partitions)
            {
                var partitionID = partition["DeviceID"].ToString();
                var offsetSec = (uint)(Convert.ToUInt64(partition["StartingOffset"]) / bytesPerSector);
                var sizeSec = (uint)(Convert.ToUInt64(partition["Size"]) / bytesPerSector);
                var wmiVolume = getPartitionVolumeWMI(partitionID);

                var diskPartition = new DiskPartition();
                diskPartition.partitionID = partitionID;
                diskPartition.mountPoint = null;
                diskPartition.offsetSec = offsetSec;
                diskPartition.sizeSec = sizeSec;

                if (wmiVolume != null)
                {
                    var mountPoint = wmiVolume["DriveLetter"].ToString();
                    var volumeGUID = PartitionManagement.getVolumeGUID(wmiVolume);

                    var volumeIdx = result.volumes.FindIndex(i => i.GUID.Equals(volumeGUID));
                    if (volumeIdx >= 0)
                    {
                        // Volume is known, update mount point only
                        var volume = result.volumes[volumeIdx];
                        volume.mountPoint = mountPoint;
                        result.volumes[volumeIdx] = volume;
                    }
                    else
                    {
                        // Volume is not known to WinAPI, but WMI sees it?
                        var volume = new DiskVolume();

                        volume.mountPoint = mountPoint;
                        volume.GUID = volumeGUID;

                        result.volumes.Add(volume);
                    }

                    diskPartition.mountPoint = mountPoint;
                }

                result.partitions.Add(diskPartition);
            }

            // Ensure partitions in order
            // [0] Preloader
            // [1] Linux
            // [2] Data
            result.partitions = result.partitions.OrderBy(o => o.offsetSec).ToList();

            return result;
        }

        /// <summary>
        /// Write image file content to the volume (raw write)
        /// </summary>
        /// <param name="volumeGUID"></param>
        /// <param name="imageFilePath"></param>
        /// <param name="maxSectors"></param>
        /// <param name="sectorSize"></param>
        /// <param name="wipeRemainder"></param>
        /// <returns></returns>
        private static bool writeVolumeFromImage(
            String physicalDiskName,
            uint startOffset,
            String imageFilePath,
            uint maxSectors,
            int sectorSize = 512,
            bool wipeRemainder = false,
            int startProgress = 0,
            int finishProgress = 0)
        {
            bool result = false;
            var maxVolumeSize = maxSectors * sectorSize; // Max allowed image file size
            var showProgress = finishProgress > startProgress && startProgress > 0;

            SafeFileHandle hImageFile = openFileWin32(imageFilePath);
            SafeFileHandle hDisk = openDiskWin32(physicalDiskName);

            if (hImageFile != null && !hImageFile.IsInvalid &&
                hDisk != null && !hDisk.IsInvalid)
            {
                long imageFileSize = getFileSizeWin32(hImageFile);

                if (imageFileSize <= maxVolumeSize)
                {
                    #region Calculate parameters

                    // Get number of full buffer fillings
                    byte[] buffer = new byte[WRITE_BUFFER_SECTORS * sectorSize];
                    var fullBuffers = imageFileSize / buffer.Length;
                    long bufferRemainder = imageFileSize - fullBuffers * buffer.Length;

                    // Calculate how many full sectors the remainder will get
                    // (Write is possible only aligned to sector length)
                    long remainderSectors = 0;

                    if (bufferRemainder > 0)
                    {
                        remainderSectors = bufferRemainder / sectorSize;
                        if (bufferRemainder % sectorSize != 0)
                        {
                            remainderSectors += 1;
                        }
                    }

                    // Calculate volume space occupied by the image (sector aligned)
                    long imageOccupiedSize = fullBuffers * buffer.Length + remainderSectors * sectorSize;

                    // If wiping option for  the remainder of volume was selected - calculate it
                    long volumeRemainder = 0;
                    long volumeRemainderFullBuffers = 0;
                    long volumeBufferRemainder = 0;
                    long volumeBufferRemainderSectors = 0;
                    if (wipeRemainder && imageOccupiedSize < maxVolumeSize)
                    {
                        volumeRemainder = maxVolumeSize - imageOccupiedSize;
                        volumeRemainderFullBuffers = volumeRemainder / buffer.Length;
                        volumeBufferRemainder = volumeRemainder - volumeRemainderFullBuffers * buffer.Length;

                        if (volumeBufferRemainder > 0)
                        {
                            volumeBufferRemainderSectors = volumeBufferRemainder / sectorSize;
                        }
                    }

                    // Progress and stats related
                    Double delta = (Double)(finishProgress - startProgress) / fullBuffers;
                    if (wipeRemainder)
                    {
                        delta = (Double)(finishProgress - startProgress) / (maxSectors / WRITE_BUFFER_SECTORS);
                    }
                    Double progress = startProgress;

                    long endStatsSize = imageFileSize;
                    if (wipeRemainder)
                    {
                        endStatsSize = maxSectors * sectorSize;
                    }

                    #endregion Calculate parameters

                    #region Write image to the volume using buffered operations

                    #region Info Log
                    Logger.Info("Starting to copy {0} image to partition with offset {1}", imageFilePath, startOffset);
                    Logger.Info("File size: {0}", imageFileSize);
                    Logger.Info("Aligned size: {0}", imageOccupiedSize);
                    Logger.Info("{0} iterations will be done with buffer size {1}", bufferRemainder == 0 ? fullBuffers : fullBuffers + 1, buffer.Length);
                    Logger.Info("");
                    #endregion Info Log

                    // Write data portion aligned to buffer size
                    for (var i = 0; i < fullBuffers; i++)
                    {
                        #region Info Log
                        Logger.Info("{0} / {1} (buffered ops) - {2} / {3}",
                            i + 1,
                            fullBuffers,
                            (i + 1) * buffer.Length,
                            imageOccupiedSize);
                        #endregion Info Log

                        var startReadSector = i * WRITE_BUFFER_SECTORS;
                        var startWriteSector = startOffset + i * WRITE_BUFFER_SECTORS;
                        if (readSectorDataFromHandleWin32(hImageFile, startReadSector, WRITE_BUFFER_SECTORS, sectorSize, ref buffer))
                        {
                            writeSectorDataToHandleWin32(hDisk, buffer, startWriteSector, WRITE_BUFFER_SECTORS, sectorSize);
                        }

                        #region Update progress
                        if (showProgress)
                        {
                            progress += delta;
                            setProgress(Convert.ToInt32(progress));

                            var stats = String.Format(
                                "{0} / {1}",
                                Utility.FormatDataSize((long)(i + 1) * WRITE_BUFFER_SECTORS * sectorSize),
                                Utility.FormatDataSize(endStatsSize));

                            setStats(stats);
                        }
                        #endregion Update progress
                    }

                    // Write the remainder of image file aligned to sector size
                    if (bufferRemainder > 0)
                    {
                        #region Info Log
                        Logger.Info("{0} / {1} (buffered ops) - {2} / {3}",
                            fullBuffers + 1,
                            fullBuffers + 1,
                            bufferRemainder,
                            imageOccupiedSize);
                        #endregion Info Log

                        Array.Clear(buffer, 0, buffer.Length);

                        var startReadSector = fullBuffers * WRITE_BUFFER_SECTORS;
                        var startWriteSector = startOffset + fullBuffers * WRITE_BUFFER_SECTORS;
                        if (readSectorDataFromHandleWin32(hImageFile, startReadSector, remainderSectors, sectorSize, ref buffer))
                        {
                            writeSectorDataToHandleWin32(hDisk, buffer, startWriteSector, remainderSectors, sectorSize);
                        }
                    }

                    #region Info Log
                    Logger.Info("OK");
                    Logger.Info("");
                    #endregion Info Log

                    #endregion

                    result = true;

                    #region Wipe the remainder of volume (if requested)

                    if (wipeRemainder && volumeRemainder > 0)
                    {
                        // Fill with zeroes area aligned to buffer size
                        for (var i = 0; i < volumeRemainderFullBuffers; i++)
                        {
                            var startSector = startOffset + (imageOccupiedSize / sectorSize) + i * WRITE_BUFFER_SECTORS;

                            writeSectorDataToHandleWin32(hDisk, buffer, startSector, WRITE_BUFFER_SECTORS, sectorSize);

                            #region Update progress
                            if (showProgress)
                            {
                                progress += delta;
                                setProgress(Convert.ToInt32(progress));

                                var stats = String.Format(
                                "{0} / {1}",
                                Utility.FormatDataSize((fullBuffers + i + 1) * WRITE_BUFFER_SECTORS * sectorSize),
                                Utility.FormatDataSize(endStatsSize));

                                setStats(stats);
                            }
                            #endregion Update progress
                        }

                        // Fill with zeroes remainder alighed to sector size
                        if (volumeBufferRemainderSectors > 0)
                        {
                            Array.Clear(buffer, 0, buffer.Length);

                            var startSector = startOffset + (imageOccupiedSize / sectorSize) + volumeRemainderFullBuffers * WRITE_BUFFER_SECTORS;
                            writeSectorDataToHandleWin32(hDisk, buffer, startSector, volumeBufferRemainderSectors, sectorSize);
                        }
                    }

                    #endregion

                    // Flush all pending changes to the volume
                    flushVolumeWin32(hDisk);
                }
                else
                {
                    Logger.Error("Image '{0}' with size {1} does not fit to partition with size {2}", imageFilePath, imageFileSize, maxVolumeSize);
                }
            }

            // Cleanup
            hImageFile.Dispose();
            hDisk.Dispose();

            return result;
        }

        /// <summary>
        /// Gets first available mount point (letter)
        /// </summary>
        /// <returns></returns>
        public static String getAvailableMountPoint()
        {
            String result = String.Empty;

            DriveInfo[] drives = DriveInfo.GetDrives();

            drives = drives.OrderBy(d => d.Name).ToArray();

            for (var letter = 'C'; letter <= 'Z'; letter++)
            {
                var driveName = String.Format("{0}:\\", letter);
                // Find first available letter
                if (!drives.Where(d => d.Name.ToUpper() == driveName).Any())
                {
                    result = letter.ToString();
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="physicalDiskName"></param>
        /// <param name="bytesPerSector"></param>
        /// <returns></returns>
        /// <remarks>Raw disk access can produce SEH exceptions that cannot be caught by .Net without tricks</remarks>
        /// <remarks>Microsoft Magazine article: https://msdn.microsoft.com/en-us/magazine/dd419661.aspx?f=255&MSPPError=-2147217396#id0070035 </remarks>
        //[HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public static bool CheckDiskCompatible(String physicalDiskName, uint bytesPerSector)
        {
            bool result = false;

            // Read MBR from disk into 512 bytes array
            byte[] mbr = new byte[0];

            try
            {
                //TODO: RemoveDebug
                Logger.Info("Reading MBR from disk {0}", physicalDiskName);

                mbr = readMBR(physicalDiskName);

                //TODO: RemoveDebug
                Logger.Info("Read successful {0} bytes", mbr.Length);
            }
            catch
            {
                Logger.Error("Unable read MBR from disk");
            }

            #region Determine data partition filesystem

            var filesystemType = PartitionTypeEnum.Unknown;

            var dataVolumeGUID = String.Empty;
            var volumes = getAllVolumesForDiskWin32(physicalDiskName);
            if (volumes != null && volumes.Count > 0)
            {
                if (volumes.Count == 1)
                {
                    // Windows 7 returns only recognized partitions
                    dataVolumeGUID = volumes[0];
                }
                else if (volumes.Count >= 3)
                {
                    // Windows 10 returns all volumes on disk
                    dataVolumeGUID = volumes[2];
                }
            }

            if (!String.IsNullOrEmpty(dataVolumeGUID))
            {
                var volumeWMI = getVolumeByGUIDWMI(dataVolumeGUID);
                var filesystem = volumeWMI["FileSystem"].ToString().ToUpper();

                switch (filesystem)
                {
                    case "FAT32":
                        filesystemType = PartitionTypeEnum.FAT32CHS;
                        break;
                    case "EXFAT":
                        filesystemType = PartitionTypeEnum.ExFAT;
                        break;
                    default:
                        break;
                }
            }

            #endregion

            if (mbr.Length == 512)
            {
                // Extract list of partitions (in the order of partitions table)
                var partitions = PartitionManagement.getMBRPartitions(mbr);

                #region Perform checks over partitions
                if (partitions != null && partitions.Count >= 3)
                {
                    var preloader = partitions[2];
                    var linux = partitions[1];
                    var data = partitions[0];

                    var preloaderSize = preloader.length * bytesPerSector;
                    var linuxSize = linux.length * bytesPerSector;
                    var dataSize = data.length * bytesPerSector;

                    var preloaderStart = preloader.offsetStart;
                    var linuxStart = linux.offsetStart;
                    var dataStart = data.offsetStart;

                    #region Info Log

                    #endregion Info Log

                    // Preloader and Linux partition need to be at least required size
                    var minPreloaderSize = PARTITION_0_SIZE - bytesPerSector; // Linux script produces 1 sector smaller partition. Accept it as well.
                    if (preloaderSize >= minPreloaderSize &&
                        linuxSize >= PARTITION_1_SIZE)
                    {
                        // Preloader needs to go before Linux partition
                        if (preloaderStart < linuxStart)
                        {
                            // Data partition needs to be first in MBR partition table with FAT32 or ExFAT type
                            if (
                                (
                                    filesystemType == PartitionTypeEnum.ExFAT ||
                                    filesystemType == PartitionTypeEnum.FAT32CHS ||
                                    filesystemType == PartitionTypeEnum.FAT32LBA
                                ) &&
                                (
                                    data.type == PartitionTypeEnum.ExFAT ||
                                    data.type == PartitionTypeEnum.FAT32CHS ||
                                    data.type == PartitionTypeEnum.FAT32LBA)
                                )
                            {
                                Logger.Info("Card is compatible with MiSTer. Partial update can be applied");
                                result = true;
                            }
                            else
                            {
                                #region Info Log

                                Logger.Info("Card is incompatible with MiSTer");
                                Logger.Info("Data partition needs to be the first in MBR and formatted with FAT32 or ExFAT");
                                Logger.Info("Use 'Full Install' option to repartition");

                                #endregion Info Log
                            }
                        }
                        else
                        {
                            #region Info Log
                            Logger.Info("Card is incompatible with MiSTer");
                            Logger.Info("U-Boot partition needs to be located before Linux partition");
                            Logger.Info("Use 'Full Install' option to repartition");

                            #endregion Info Log
                        }
                    }
                    else
                    {
                        #region Info Log
                        Logger.Info("Card is incompatible with MiSTer");

                        if (preloaderSize < PARTITION_1_SIZE)
                        {
                            Logger.Info("U-Boot partition (1) needs to be at least {0} bytes", PARTITION_0_SIZE);
                        }

                        if (linuxSize < PARTITION_1_SIZE)
                        {
                            Logger.Info("Linux partition (2) needs to be at least {0} bytes", PARTITION_0_SIZE);
                        }

                        Logger.Info("Use 'Full Install' option to repartition");
                        #endregion Info Log
                    }
                }
                else
                {
                    #region Info Log
                    Logger.Info("Card is incompatible with MiSTer");
                    Logger.Info("At least 3 partitions required. Found only {0}", partitions.Count);
                    Logger.Info("Use 'Full Install' option to repartition");
                    #endregion Info Log
                }
                #endregion
            }

            return result;
        }

        private static void setProgress(int value)
        {
            MainForm.SetProgress(value);
        }

        private static void setStats(String message)
        {
            MainForm.SetStats(message);
        }

        #endregion
    }
}
