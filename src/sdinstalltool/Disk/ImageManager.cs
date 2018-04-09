using SDInstallTool.Helpers;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static ICSharpCode.SharpZipLib.Zip.FastZip;

namespace SDInstallTool
{
    public static class ImageManager
    {
        #region Constants

        // U-boot partition image (/files/linux/uboot.img) will be written to preloader partition
        // All files and subfolders (files/*) will be copied to FAT partition without any changes
        public static readonly String files = "files";
        public static readonly String UbootPartitionFileName = Path.Combine(Path.Combine(files, "linux"), "uboot.img");

        #endregion

        #region Types

        struct PartitionImage
        {
            public String fileName;
            public Int64 size;

            public PartitionImage(String fileName, Int64 size)
            {
                this.fileName = fileName;
                this.size = size;
            }
        }

        #endregion

        #region Fields

        private static List<PartitionImage> partitionImages = new List<PartitionImage>();

        #endregion

        #region Constructors

        static ImageManager()
        {
            partitionImages.Add(new PartitionImage(UbootPartitionFileName, 1024 * 1024));
        }

        #endregion

        #region Properties

        private static String getUootImagePath()
        {
            return UbootPartitionFileName;
        }

        #endregion

        #region Methods
        public static String checkUpdatePackage()
        {
            int successCounter = 0;
            String result = String.Empty;
            StringBuilder sb = new StringBuilder();

            var currentFolder = Directory.GetCurrentDirectory();

            foreach (PartitionImage image in partitionImages)
            {
                var fileName = image.fileName;
                var size = image.size;

                var filePath = Path.Combine(currentFolder, fileName);

                if (!File.Exists(filePath))
                {
                    sb.AppendFormat("File '{0}' is missing\r\n", fileName);
                    break;
                }

                Int64 fileSizeInBytes = new FileInfo(fileName).Length;

                if (fileSizeInBytes > size)
                {
                    sb.AppendFormat("File '{0}' has size={1} but expected no more than {2}\r\n", fileName, fileSizeInBytes, size);
                    break;
                }

                successCounter += 1;
            }

            if (successCounter != partitionImages.Count)
            {
                result = String.Format("Update in folder {0} has errors: \r\n{1}", currentFolder, sb.ToString());
            }

            return result;
        }

        public static bool Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            bool result = CopyAll(diSource, diTarget);

            return result;
        }

        public static bool CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            bool result = false;

            try
            {
                // Replicate directory structure
                Directory.CreateDirectory(target.FullName);
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    // React on Cancellation
                    if (MainForm.IsCancelTriggered())
                        return false;

                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                }

                CustomFileCopier copier = new CustomFileCopier();
                copier.SetFolders(source.FullName, target.FullName);
                copier.SetProgressLimits(20, 100);
                copier.CopyData();

                result = true;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return result;
        }

        public static bool checkUpdateFiles()
        {
            var currentFolder = Directory.GetCurrentDirectory();
            var DirPath = Path.Combine(currentFolder, files);

            return Directory.Exists(DirPath);
        }

        public static bool copyUpdateFiles(String volumePath)
        {
            bool result = false;
            if (!volumePath.EndsWith("\\"))
                volumePath = volumePath + "\\";

            var currentFolder = Directory.GetCurrentDirectory();
            var srcPath = Path.Combine(currentFolder, files);
            var dstPath = volumePath;

            if (Directory.Exists(srcPath))
            {
                result = Copy(srcPath, dstPath);
            }

            return result;
        }

        #endregion
    }
}
