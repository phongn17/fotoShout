using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Utils {
    public class ImageUtils {
        public const string IMG_EXTS_DEFAULT = "*.jpg|*.png";
        public const string IMG_TYPES_DEFAULT = "*image/jpeg|image/png";
        public const int THUMB_WIDTH = 250;
        
        public static string GetImageExts() {
            string imageExts = System.Configuration.ConfigurationManager.AppSettings["ImageExts"];
            if (string.IsNullOrEmpty(imageExts))
                imageExts = ImageUtils.IMG_EXTS_DEFAULT;

            return imageExts;
        }

        public static string[] GetImageTypes()
        {
            string imageTypes = System.Configuration.ConfigurationManager.AppSettings["ImageTypes"];
            if (string.IsNullOrEmpty(imageTypes))
                imageTypes = ImageUtils.IMG_TYPES_DEFAULT;

            return imageTypes.Split('|');
        }

        public static byte[] GetBinaryImage(string path, ImageFormat imgFormat) {
            using (var image = Image.FromFile(path))
            using (var memoryStream = new MemoryStream()) {
                image.Save(memoryStream, imgFormat);
                return memoryStream.GetBuffer();
            }
        }

        public static bool CreateThumbnailFileIfAny(string imagePath, string thumbPath) {
            if (File.Exists(thumbPath))
                return true;

            Image image = null;
            Image thumbnail = null;
            try {
                string temp = ConfigurationManager.AppSettings[Constants.AS_THUMBNAILWIDTH];
                int thumbWidth = 0;
                bool ret = false;
                if (!string.IsNullOrEmpty(temp))
                    ret = int.TryParse(temp.Trim(), out thumbWidth);
                if (!ret)
                    thumbWidth = ImageUtils.THUMB_WIDTH;

                image = Image.FromFile(imagePath);
                int thumbHeight = image.Height * thumbWidth / image.Width;
                thumbnail = image.GetThumbnailImage(thumbWidth, thumbHeight, () => false, IntPtr.Zero);
                thumbnail.Save(thumbPath);
            }
            catch (Exception) {
                return false;
            }
            finally {
                if (image != null)
                    image.Dispose();
                if (thumbnail != null)
                    thumbnail.Dispose();
            }

            return true;
        }
    }
}
