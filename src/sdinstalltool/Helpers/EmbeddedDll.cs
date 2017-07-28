using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

/// <summary>
/// Helper class to simplify loading native Dlls stored as Assembly resources
/// </summary>
namespace SDInstallTool.Helpers
{
    public class EmbeddedDll
    {
        #region Fields
    
        private static String tempFolder;

        #endregion

        #region Win32 Interop

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(String lpFileName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetDllDirectory(String lpPathName);

        #endregion Win32 Interop

        #region Public methods

        /// <summary>
        /// Extract DLLs from resources to temporary folder
        /// </summary>
        /// <param name="dllName">name of DLL file to create (including dll suffix)</param>
        public static void ExtractEmbeddedDlls(string dllName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();
            AssemblyName an = assembly.GetName();

            var resourceName = ResolveResourceName(dllName);

            // Load Dll from resources
            Stream dllStream = assembly.GetManifestResourceStream(resourceName);
            byte[] resourceBytes = ReadToEnd(dllStream);


            // The temporary folder holds one or more of the temporary DLLs
            // It is made "unique" to avoid different versions of the DLL or architectures.
            var tempFolderName = String.Format("{0}.{1}.{2}", an.Name, an.ProcessorArchitecture, an.Version);

            tempFolder = Path.Combine(Path.GetTempPath(), tempFolderName);
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            #region Add to PATH variable

            // Add the temporary dirName to the PATH environment variable (at the head!)
            string path = Environment.GetEnvironmentVariable("PATH");
            string[] pathPieces = path.Split(';');
            bool found = false;
            foreach (string pathPiece in pathPieces)
            {
                if (pathPiece == tempFolder)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Environment.SetEnvironmentVariable("PATH", tempFolder + ";" + path);
            }

            #endregion Add to PATH variable

            // See if the file exists, avoid rewriting it if not necessary
            string dllPath = Path.Combine(tempFolder, dllName);
            bool rewrite = true;

            if (File.Exists(dllPath))
            {
                byte[] existing = File.ReadAllBytes(dllPath);

                if (resourceBytes.SequenceEqual(existing))
                {
                    rewrite = false;
                }
            }

            if (rewrite)
            {
                File.WriteAllBytes(dllPath, resourceBytes);
            }
        }

        #endregion Public methods

        #region Helper methods

        /// <summary>
        /// managed wrapper around LoadLibrary
        /// </summary>
        /// <param name="dllName"></param>
        static public IntPtr LoadDll(string dllName)
        {
            IntPtr result = IntPtr.Zero;

            if (!String.IsNullOrEmpty(tempFolder))
            {
                IntPtr hLibrary = LoadLibrary(dllName);
                if (hLibrary != IntPtr.Zero)
                {
                    result = hLibrary;
                }
            }

            return result;
        }

        protected static String ResolveResourceName(String resourceName)
        {
            String result = String.Empty;

            Assembly assembly = Assembly.GetExecutingAssembly();
            List<String> resourceNames = new List<String>(assembly.GetManifestResourceNames());

            result = resourceName.Replace(@"/", ".");
            result = resourceNames.FirstOrDefault(r => r.Contains(result));

            return result;
        }

        public static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }

                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        #endregion Helper methods
    }
}
