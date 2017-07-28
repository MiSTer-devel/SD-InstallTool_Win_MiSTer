using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SDInstallTool.Helpers
{
    public static class Logger
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string message);

        #region Log errors
        public static void Log(String message)
        {
            // Dispatch message directly to global application log
            MainForm.LogMessage(message);

            // Output to system debug log as well
            OutputDebugString("MiSTerSD:" + message);
        }

        public static void Log(String format, params object[] args)
        {
            var message = String.Format(format, args);

            Log(message);
        }

        public static void Info(String format, params object[] args)
        {
            var message = String.Format(format, args);

            Info(message);
        }

        public static void Error(String format, params object[] args)
        {
            var message = String.Format(format, args);

            Error(message);
        }

        public static void Info(String message)
        {
            String errMessage = "[Info] " + message;

            Log(message);
        }

        public static void Error(String message)
        {
            String errMessage = "[Error] " + message;

            Log(message);
        }

        public static void Error(int code, String message)
        {
            Log("ErrorCode: 0x{0}, Message: '{1}'", code.ToString("x"), message);
        }

        public static void Error(Win32Exception e)
        {
            Error(e.ErrorCode, e.Message);
        }

        #endregion

        #region Win32 Helpers

        public static void LogWin32Error()
        {
            int errorCode = Marshal.GetLastWin32Error();
            Win32Exception exception = new Win32Exception(errorCode);
            String error = exception.Message;

            Error(exception);
        }

        #endregion
    }
}
