using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using SDInstallTool.Helpers;
using System.IO;

namespace SDInstallTool
{
    public static partial class DiskManagement
    {
        private static ManagementObject geDiskDriveWMI(String physicalDiskName)
        {
            String query = String.Format("SELECT * FROM Win32_DiskDrive WHERE DeviceID = '{0}'", physicalDiskName.Replace("\\", "\\\\"));
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", query);
            var queryCollection = from ManagementObject x in searcher.Get() select x;
            var disk = queryCollection.FirstOrDefault();

            return disk;
        }

        private static bool isDiskRemovableWMI(ManagementObject queryObj)
        {
            bool result = false;

            try
            {
                var mediaType = queryObj["MediaType"].ToString();
                if (mediaType.Contains("Removable Media"))
                    result = true;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return result;
        }

        private static String getDiskDescriptionWMI(ManagementObject queryObj)
        {
            String result = String.Empty;

            try
            {
                var mediaType = queryObj["MediaType"];
                var physicalDeviceName = queryObj["DeviceID"].ToString();
                var model = queryObj["Model"];
                var size = Convert.ToUInt64(queryObj["Size"]);

                var sizeValue = Convert.ToDouble(size) / Convert.ToDouble(1024 * 1024 * 1024); // Size in gigabytes

                result = String.Format("{0} - {1:.00}Gb", model, sizeValue);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return result;
        }


        public static List<ManagementObject> getAllVolumesWMI()
        {
            var result = new List<ManagementObject>();

            var query = "SELECT * FROM Win32_Volume";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", query);
            foreach (ManagementObject volume in searcher.Get())
            {
                result.Add(volume);
            }

            return result;
        }

        public static List<String> getAllVolumesGUIDNamesWMI()
        {
            var result = new List<String>();

            var volumes = getAllVolumesWMI();
            foreach (ManagementObject volume in volumes)
            {
                var volumeGUID = volume["DeviceID"].ToString();
                volumeGUID = volumeGUID.TrimEnd('\\');
                result.Add(volumeGUID);
            }

            return result;
        }

        public static ManagementObject getPartitionLogicDiskWMI(String partitionID)
        {
            ManagementObject result = null;

            var query = String.Format("ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{0}'}} WHERE AssocClass = Win32_LogicalDiskToPartition", partitionID);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", query);
            foreach (ManagementObject volume in searcher.Get())
            {
                result = volume;
                break;
            }

            return result;
        }

        public static ManagementObject getPartitionVolumeWMI(String partitionID)
        {
            ManagementObject result = null;

            ManagementObject logicDisk = getPartitionLogicDiskWMI(partitionID);
            if (logicDisk != null)
            {
                var diskName = logicDisk["DeviceID"].ToString();
                result = getVolumeByNameWMI(diskName);
            }

            return result;
        }

        public static ManagementObject getVolumeByNameWMI(String volumeName)
        {
            ManagementObject result = null;

            var query = String.Format("SELECT * FROM Win32_Volume WHERE DriveLetter = '{0}'", volumeName);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", query);
            foreach (ManagementObject volume in searcher.Get())
            {
                result = volume;
                break;
            }

            return result;
        }

        public static ManagementObject getVolumeByGUIDWMI(String volumeGUID)
        {
            ManagementObject result = null;

            var query = String.Format("SELECT * FROM Win32_Volume WHERE DeviceID = '{0}'", volumeGUID.Replace("\\", "\\\\"));
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", query);
            foreach (ManagementObject volume in searcher.Get())
            {
                result = volume;
                break;
            }

            return result;
        }
    }
}
