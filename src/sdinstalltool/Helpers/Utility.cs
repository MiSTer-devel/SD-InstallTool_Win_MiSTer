using System;
using System.Drawing;
using System.Reflection;

namespace SDInstallTool
{
    internal class Utility
    {
        public static Icon GetAppIcon()
        {
            var fileName = Assembly.GetEntryAssembly().Location;

            var hLibrary = NativeMethods.LoadLibrary(fileName);
            if (!hLibrary.Equals(IntPtr.Zero))
            {
                var hIcon = NativeMethods.LoadIcon(hLibrary, "#32512");
                if (!hIcon.Equals(IntPtr.Zero))
                    return Icon.FromHandle(hIcon);
            }
            return null; //no icon was retrieved
        }

        public static String FormatDataSize(uint value)
        {
            String result = FormatDataSize((long)value);

            return result;
        }

        public static String FormatDataSize(long value)
        {
            String result = String.Empty;

            string[] Suffix = { " B", " kB", " MB", " GB", " TB" };
            double dblSByte = value;
            int i;
            for (i = 0; i < Suffix.Length && value >= 1024; i++, value /= 1024)
            {
                dblSByte = value / 1024.0;
            }
            result = $"{dblSByte:0.##}{Suffix[i]}";

            return result;
        }
    }
}
