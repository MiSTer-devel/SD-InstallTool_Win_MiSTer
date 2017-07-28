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

        public static readonly String UbootPartitionFileName = "uboot.img";
        public static readonly String LinuxPartitionFileName = "linux.img";
        public static readonly String MisterArchiveFileName = "mister.zip";

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
            partitionImages.Add(new PartitionImage(LinuxPartitionFileName, 500 * 1024 * 1024));
        }

        #endregion

        #region Properties

        private static String getUootImagePath()
        {
            return UbootPartitionFileName;
        }

        private static String getLinuxImagePath()
        {
            return LinuxPartitionFileName;
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

        public static bool checkMisterPackage()
        {
            bool result = false;

            var fileName = MisterArchiveFileName;
            var currentFolder = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(currentFolder, fileName);

            if (File.Exists(filePath))
            {
                Int64 fileSizeInBytes = new FileInfo(fileName).Length;

                if (fileSizeInBytes > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public static bool unpackMisterPackage(String volumePath)
        {
            bool result = false;

            var fileName = MisterArchiveFileName;
            var currentFolder = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(currentFolder, fileName);

            try
            {
                FastZip fastZip = new FastZip();
                string filter = null; // Don't filter any files at all
                
                //fastZip.ExtractZip(filePath, volumePath, filter);
                fastZip.ExtractZip(filePath, volumePath, Overwrite.Always, null, filter, filter, false);

                result = true;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            return result;
        }

        #endregion
    }
}
