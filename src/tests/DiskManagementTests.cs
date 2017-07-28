using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDInstallTool;

namespace Tests
{
    [TestClass]
    public class DiskManagementTests
    {
        [TestMethod]
        public void getAvailableMountPointTest()
        {
            var mountPoint = DiskManagement.getAvailableMountPoint();
            Assert.AreNotEqual("C:", mountPoint);
        }

        [TestMethod]
        public void getAllVolumesForDiskWin32Test()
        {
            var res = DiskManagement.getAllVolumesForDiskWin32("\\\\.\\PhysicalDisk1");
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Count > 0);
        }
    }
}
