using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SDInstallTool
{
    public static partial class PartitionManagement
    {
        #region Types

        private const int FMIFS_REMOVABLE = 0xB;
        private const int FMIFS_HARDDISK = 0xC;

        [Flags]
        private enum CallbackCommand
        {
            PROGRESS,
            DONEWITHSTRUCTURE,
            UNKNOWN2,
            UNKNOWN3,
            UNKNOWN4,
            UNKNOWN5,
            INSUFFICIENTRIGHTS,
            UNKNOWN7,
            DISKLOCKEDFORACCESS,
            UNKNOWN9,
            UNKNOWNA,
            DONE,
            UNKNOWNC,
            UNKNOWND,
            OUTPUT,
            STRUCTUREPROGRESS
        }

        #endregion

        private delegate Int32 FormatCallBackDelegate(
            CallbackCommand callBackCommand,
            int subActionCommand,
            IntPtr action);

        [DllImport("FMIFS.dll", EntryPoint = "FormatEx", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void FormatEx(
            String driveLetter,
            int mediaFlag,
            String fsType,
            String label,
            Int32 quickFormat,
            Int32 clusterSize,
            FormatCallBackDelegate callBackDelegate);

        public static void FormatFMIFS(string name)
        {
            FormatCallBackDelegate fcb = new FormatCallBackDelegate(formatCallBack);
            FormatEx(name, FMIFS_REMOVABLE, "FAT32", "MiSTer Data", 0, 0, fcb);
        }

        private static Int32 formatCallBack(CallbackCommand callBackCommand, int subActionCommand, IntPtr action)
        {
            switch (callBackCommand)
            {
                case CallbackCommand.DISKLOCKEDFORACCESS:
                    break;
                case CallbackCommand.PROGRESS:
                    string percent = Convert.ToString(action);
                    break;
                case CallbackCommand.OUTPUT:
                    string output = Convert.ToString(action);
                    break;
                case CallbackCommand.DONE:
                    string status = Convert.ToString(action);
                    break;
            }
            return 1;
        }
    }
}
