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
        // All other files (files/*) will be copied to FAT partition without any changes
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

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Logger.Info("Copying "+ Path.Combine(target.FullName, fi.Name));
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static bool checkUpdateFiles()
        {
            var currentFolder = Directory.GetCurrentDirectory();
            var DirPath = Path.Combine(currentFolder, files);

            return Directory.Exists(DirPath);
        }

        public static bool copyUpdateFiles(String volumePath)
        {
            var currentFolder = Directory.GetCurrentDirectory();
            var srcPath = Path.Combine(currentFolder, files);
            var dstPath = volumePath;

            if (Directory.Exists(srcPath))
            {
                Copy(srcPath, dstPath);
                return true;
            }

            return false;
        }

        #endregion
    }
}
