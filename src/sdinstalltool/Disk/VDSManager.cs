using SDInstallTool.Helpers;
using System;
using System.Runtime.InteropServices;

namespace SDInstallTool
{
    public static class VDSManager
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool FormatDelegate(
            [In] String letter,
            [In] String pwszFileSystemTypeName,
            [In] UInt32 ulDesiredUnitAllocationSize,
            [In] String pwszLabel
            );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool FormatVolumeDelegate(
            [In] String pwszVolumeID,
            [In] String pwszFileSystemTypeName,
            [In] UInt32 ulDesiredUnitAllocationSize,
            [In] String pwszLabel
            );

        [DllImport("misterhelper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool Format(
            [In] String letter,
            [In] String pwszFileSystemTypeName,
            [In] UInt32 ulDesiredUnitAllocationSize,
            [In] String pwszLabel
            );

        [DllImport("misterhelper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool FormatVolume(
            [In] String pwszVolumeID,
            [In] String pwszFileSystemTypeName,
            [In] UInt32 ulDesiredUnitAllocationSize,
            [In] String pwszLabel
            );

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public extern static IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        public static bool format(char letter, String fileSystemName, UInt32 allocationSize, String label)
        {
            bool result = false;

            string helperDLLName = "misterhelper.dll";
            EmbeddedDll.ExtractEmbeddedDlls(helperDLLName);
            IntPtr hDll = EmbeddedDll.LoadDll(helperDLLName);
            if (hDll != IntPtr.Zero)
            {
                IntPtr pAddrFormatFunction = GetProcAddress(hDll, "Format");
                if (!IntPtr.Equals(pAddrFormatFunction, IntPtr.Zero))
                {
                    FormatDelegate formatDelegate = (FormatDelegate)Marshal.GetDelegateForFunctionPointer(pAddrFormatFunction, typeof(FormatDelegate));
                    result = formatDelegate(letter.ToString(), fileSystemName, allocationSize, label);
                }
                else
                {
                    Logger.Error("Unable to resolve 'Format' function in misterhelper.dll");
                }

                FreeLibrary(hDll);
            }
            else
            {
                Logger.Error("Unable to load 'misterhelper.dll'");
            }
            
            /*
             * Call from direct reference
            {
                Format(letter.ToString(), fileSystemName, allocationSize, label);
            }
            */

            return result;
        }

        public static bool formatDirect(char letter, String fileSystemName, UInt32 allocationSize, String label)
        {
            bool result = Format(letter.ToString(), fileSystemName, allocationSize, label);
            return result;
        }

        public static bool formatVolume(String volumeID, String fileSystemName, UInt32 allocationSize, String label)
        {
            bool result = false;

            string helperDLLName = "misterhelper.dll";
            EmbeddedDll.ExtractEmbeddedDlls(helperDLLName);
            IntPtr hDll = EmbeddedDll.LoadDll(helperDLLName);
            if (hDll != IntPtr.Zero)
            {
                IntPtr pAddrFormatFunction = GetProcAddress(hDll, "FormatVolume");
                if (!IntPtr.Equals(pAddrFormatFunction, IntPtr.Zero))
                {
                    FormatVolumeDelegate formatDelegate = (FormatVolumeDelegate)Marshal.GetDelegateForFunctionPointer(pAddrFormatFunction, typeof(FormatVolumeDelegate));
                    result = formatDelegate(volumeID, fileSystemName, allocationSize, label);
                }
                else
                {
                    Logger.Error("Unable to resolve 'FormatVolume' function in misterhelper.dll");
                }

                FreeLibrary(hDll);
            }
            else
            {
                Logger.Error("Unable to load 'misterhelper.dll'");
            }

            /*
             * Call from direct reference
            {
                Format(letter.ToString(), fileSystemName, allocationSize, label);
            }
            */

            return result;
        }

        public static bool formatVolumeDirect(String volumeID, String fileSystemName, UInt32 allocationSize, String label)
        {
            bool result = FormatVolume(volumeID, fileSystemName, allocationSize, label);

            return result;
        }
    }
}
