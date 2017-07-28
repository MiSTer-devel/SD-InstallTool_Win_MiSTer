using System;
using System.Text;
using System.Management;

namespace SDInstallTool
{
    public static partial class DiskManagement
    {
        public static String getPhysicalDiskName(int diskNo)
        {
            String result = String.Format("\\\\.\\PhysicalDrive{0}", diskNo);

            return result;
        }

        public static String getVolumeName(string mountPoint)
        {
            String result = String.Format("\\\\.\\{0}", mountPoint);

            return result;
        }

        public static String dumpWMIObject(ManagementObject obj)
        {
            StringBuilder sb = new StringBuilder();

            PropertyDataCollection props = obj.Properties;
            foreach (PropertyData prop in props)
            {
                var propertyValue = String.Format("{0} ({1}) = {2}", prop.Name, prop.Type, prop.Value);
                sb.AppendLine(propertyValue);
            }

            return sb.ToString();
        }
    }
}
