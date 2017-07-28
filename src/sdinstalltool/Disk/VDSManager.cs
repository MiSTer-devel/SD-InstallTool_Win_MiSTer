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

        [DllImport("misterhelper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool Format(
            [In] String letter,
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
                FormatDelegate formatDelegate = (FormatDelegate)Marshal.GetDelegateForFunctionPointer(pAddrFormatFunction, typeof(FormatDelegate));
                result = formatDelegate(letter.ToString(), fileSystemName, allocationSize, label);

                FreeLibrary(hDll);
            }
            
            /*
             * Call from direct reference
            {
                Format(letter.ToString(), fileSystemName, allocationSize, label);
            }
            */

            return result;
        }
    }
}
