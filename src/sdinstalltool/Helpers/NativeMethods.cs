using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace SDInstallTool
{
    public enum EMoveMethod : int
    {
        Begin = 0,
        Current = 1,
        End = 2
    }

    public static class NativeMethods 
    {
        internal const uint OPEN_EXISTING = 3;
        internal const uint GENERIC_WRITE = (0x40000000);
        internal const uint GENERIC_READ = 0x80000000;

        internal const uint FSCTL_LOCK_VOLUME = 0x00090018;
        internal const uint FSCTL_UNLOCK_VOLUME = 0x0009001c;
        internal const uint FSCTL_IS_VOLUME_MOUNTED = 0x00090028;
        internal const uint FSCTL_DISMOUNT_VOLUME = 0x00090020;
        internal const uint FSCTL_ALLOW_EXTENDED_DASD_IO = 0x00090083;

        internal const uint FILE_SEEK_BEGIN = 0;
        internal const uint FILE_SEEK_CURRENT = 1;
        internal const uint FILE_SEEK_END = 2;

        internal const uint FILE_SHARE_READ = 0x1;
        internal const uint FILE_SHARE_WRITE = 0x2;
        internal const uint FILE_SHARE_DELETE = 0x4;
        internal const uint FILE_SHARE_VALID_FLAGS = 0x7;

        internal const uint FILE_FLAG_NO_BUFFERING = 0x20000000;

        internal const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        internal const uint IOCTL_DISK_GET_DRIVE_GEOMETRY = 0x70000;
        internal const uint IOCTL_DISK_GET_DRIVE_GEOMETRY_EX = 0x700a0;
        internal const uint IOCTL_DISK_DELETE_DRIVE_LAYOUT = 0x7c100;
        internal const uint IOCTL_DISK_UPDATE_PROPERTIES = 0x0070140;
        internal const uint IOCTL_DISK_GET_DRIVE_LAYOUT_EX = 0x70050;

        internal const uint IOCTL_STORAGE_GET_DEVICE_NUMBER = 0x2d1080;

        // Mount manager controls codes - https://msdn.microsoft.com/en-us/library/windows/hardware/ff561593(v=vs.85).aspx
        internal const uint IOCTL_MOUNTMGR_SET_AUTO_MOUNT = 0x6dc040;
        internal const uint IOCTL_MOUNTMGR_QUERY_AUTO_MOUNT = 0x6d003c;
        internal const uint IOCTL_MOUNTMGR_CREATE_POINT = 0x6dc000;

        internal const uint IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS = 0x00560000;

        internal const uint BCM_SETSHIELD = 0x160C;

        // System error codes https://msdn.microsoft.com/en-us/library/windows/desktop/ms681382(v=vs.85).aspx
        internal const byte ERROR_SUCCESS = 0x00;
        internal const byte ERROR_BUSY_DRIVE = 0x8E;
        internal const byte ERROR_SAME_DRIVE = 0x8F;
        internal const byte ERROR_DIR_NOT_ROOT = 0x90;
        internal const byte ERROR_DIR_NOT_EMPTY = 0x91;

        internal const uint INVALID_SET_FILE_POINTER = 0xFFFFFFFF;

        internal static readonly string MOUNTMGR_DOS_DEVICE_NAME = "\\\\.\\MountPointManager";

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern internal IntPtr LoadIcon(IntPtr hInstance, string lpIconName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern internal IntPtr LoadLibrary(string lpFileName);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern internal int SetFilePointer([In] SafeFileHandle hFile, [In] int lDistanceToMove,  ref int lpDistanceToMoveHigh, [In] EMoveMethod dwMoveMethod);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern internal  SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32", SetLastError = true)]
        static extern internal int ReadFile(SafeFileHandle handle, byte[] bytes, int numBytesToRead, out int numBytesRead, IntPtr overlapped_MustBeZero);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern internal int WriteFile(SafeFileHandle handle, byte[] bytes, int numBytesToWrite, out int numBytesWritten, IntPtr overlapped_MustBeZero);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        static extern internal bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode, byte[] lpInBuffer, int nInBufferSize, byte[] lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = false, CharSet = CharSet.Auto)]
        public static extern bool DeviceIoControl(SafeFileHandle device, uint dwIoControlCode, IntPtr inBuffer, uint inBufferSize, IntPtr outBuffer, uint outBufferSize, ref uint bytesReturned, IntPtr overlapped);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        static extern internal bool CloseHandle(SafeFileHandle handle);

        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, IntPtr lParam);

    }
}