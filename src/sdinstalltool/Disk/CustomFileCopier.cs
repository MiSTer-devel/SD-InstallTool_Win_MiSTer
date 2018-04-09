using System.IO;
using System.Threading;
using System.Collections.Generic;
using SDInstallTool.Helpers;

namespace SDInstallTool
{
    class CustomFileCopier
    {
        #region Fields
        protected string m_sourcePath;
        protected string m_destinationPath;
        protected int m_progressBegin = 0;
        protected int m_progressEnd = 100;

        protected long m_totalSize = 0;
        protected long m_bytesCopied = 0;
        protected List<string> m_listFilePaths = new List<string>();
        #endregion

        #region Methods
        public bool SetFolders(string sourcePath, string destinationPath)
        {
            bool result = false;

            m_listFilePaths.Clear();
            m_totalSize = 0;
            m_bytesCopied = 0;

            //string fullSourcePath = Path.GetFullPath(sourcePath);
            //string fullDestinationPath = Path.GetFullPath(sourcePath);

            bool error = false;
            if (Directory.Exists(sourcePath))
            {
                try
                {
                    m_totalSize = GetDirectorySize(sourcePath, "", true);
                }
                catch
                {
                    Logger.Error("Unable to calculate directory '{0}' size", sourcePath);
                    error = true;
                }

                if (!error && Directory.Exists(destinationPath))
                {
                    m_sourcePath = sourcePath;
                    m_destinationPath = destinationPath;

                    result = true;
                }
            }

            return result;
        }

        public void SetProgressLimits(int beginLimit, int endLimit)
        {
            if (beginLimit < 0)
                beginLimit = 0;
            if (endLimit > 100)
                endLimit = 100;

            m_progressBegin = beginLimit;
            m_progressEnd = endLimit;
        }

        public void CopyData()
        {
            if (string.IsNullOrEmpty(m_sourcePath) || string.IsNullOrEmpty(m_destinationPath))
                return;

            m_bytesCopied = 0;

            foreach (var file in m_listFilePaths)
            {
                // React on Cancellation
                if (MainForm.IsCancelTriggered())
                    return;

                try
                {
                    string sourcePath = Path.Combine(m_sourcePath, file);
                    string destinationPath = Path.Combine(m_destinationPath, file);

                    CopyFile(sourcePath, destinationPath);
                }
                catch
                {
                    Logger.Error("Unable to copy file {0}", file);
                }
            }

            SetProgress(m_progressEnd);

            // Clear last filename copied in status bar
            SetStats("");
        }

        #endregion

        #region Helper methods
        protected long GetDirectorySize(string directoryPath, string prefixFolder, bool recursive = true, bool isRoot = true)
        {
            long result = 0;

            string prefixedFolder = Path.Combine(prefixFolder, directoryPath);


            // Return 0 if Directory does not exist
            if (!Directory.Exists(prefixedFolder))
                return result;

            var currentDirectory = new DirectoryInfo(prefixedFolder);

            // Add size of files in the Current Directory to main size.
            foreach (var file in currentDirectory.GetFiles())
            {
                Interlocked.Add(ref result, file.Length);

                string filePath;
                if (isRoot)
                    filePath = file.Name;
                else
                    filePath = Path.Combine(directoryPath, file.Name);

                m_listFilePaths.Add(filePath);
            }

            // Loop on Sub-directories recursively to calculate sizes there
            if (recursive)
            {
                foreach (var subDirectory in currentDirectory.GetDirectories())
                {
                    Interlocked.Add(ref result, GetDirectorySize(subDirectory.Name, prefixedFolder, recursive, false));
                }
            }

            return result;
        }

        protected void CopyFile(string sourcePath, string destinationPath)
        {
            byte[] buffer = new byte[1024 * 1024]; // 1MB caching buffer

            using (FileStream source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            {
                long fileLength = source.Length;
                using (FileStream dest = new FileStream(destinationPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    SetStats(string.Format("{0}", Path.GetFileName(sourcePath)));

                    int currentBlockSize = 0;
                    while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // React on Cancellation
                        if (MainForm.IsCancelTriggered())
                            return;

                        dest.Write(buffer, 0, currentBlockSize);

                        m_bytesCopied += currentBlockSize;
                        double progressPercentage = (double)m_bytesCopied / (double)m_totalSize;
                        int progressValue = m_progressBegin + (int)((m_progressEnd - m_progressBegin) * progressPercentage);
                        SetProgress(progressValue);
                    }
                }
            }
        }

        private static void SetProgress(int value)
        {
            MainForm.SetProgress(value);
        }

        private static void SetStats(string message)
        {
            MainForm.SetStats(message);
        }

        #endregion
    }
}
