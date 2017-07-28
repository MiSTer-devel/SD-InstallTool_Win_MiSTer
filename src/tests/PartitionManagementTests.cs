using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDInstallTool;
using System.Runtime.InteropServices;
using System.Management;

namespace Tests
{
    [TestClass]
    public class PartitionManagementTests
    {
        [TestMethod]
        public void TestMBRPartitionEntryStructure()
        {
            var mbrPartitionStructure = new MBRPartitionEntry();
            var realSize = Marshal.SizeOf(mbrPartitionStructure);
            var expected = 16;
            var message = String.Format("Expected: {0}, found: {1}", expected, realSize);

            Assert.AreEqual(expected, realSize, message);
        }

        [TestMethod]
        public void TestMBRStructure()
        {
            var mbrStructure = new MBR(0);
            var realSize = Marshal.SizeOf(mbrStructure);
            var expected = 512;
            var message = String.Format("Expected: {0}, found: {1}", expected, realSize);

            Assert.AreEqual(expected, realSize, message);
        }

        [TestMethod]
        public void TestMBRSerialization()
        {
            var mbrStructure = new MBR(0);
            var bytes = PartitionManagement.getBytes(mbrStructure);
            var size = sizeof(byte) * bytes.Length;
            var expected = 512;
            var message = String.Format("Expected: {0}, found: {1}", expected, size);

            Assert.AreEqual(expected, size, message);
        }

        [TestMethod]
        public void TestFAT32Format()
        {
            PartitionManagement.formatFAT32PartitionWMI("H:\\");
        }

        [TestMethod]
        public void TestWMIPartitions()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDrive");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                var query = String.Format("ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{0}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition", queryObj["DeviceID"]);
                ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("root\\CIMV2", query);
                foreach (var partition in searcher2.Get())
                {
                    Console.WriteLine(partition["Name"]);
                }
            }
        }
    }
}
