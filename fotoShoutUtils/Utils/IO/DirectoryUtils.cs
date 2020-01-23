using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FotoShoutUtils.Utils.IO {
    public class DirectoryUtils {
        
        public static bool IsValidFolder(string folder) {
            if (string.IsNullOrEmpty(folder))
                return false;

            return Directory.Exists(folder);
        }

        public static bool IsValidFolderOrEmpty(string folder) {
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                return false;

            return true;
        }

        public static string GenerateDirectories(string folder) {
            DirectoryInfo dirInfo = new DirectoryInfo(folder);
            if (!dirInfo.Exists) {
                dirInfo = System.IO.Directory.CreateDirectory(folder);
                if (dirInfo == null)
                    return string.Format("The \"{0}\" folder can not be created.", folder);
            }

            dirInfo = DirectoryUtils.CreateSubFolder(folder, Constants.STR_UNCLAIMED);
            if (dirInfo == null)
                return string.Format("An unclaimed folder can not be created in the \"{0}\" folder.", folder);

            return "";
        }

        public static DirectoryInfo CreateSubFolder(string root, string subFolder) {
            string folder = string.IsNullOrEmpty(root) ? "" : root;
            if (!string.IsNullOrEmpty(folder)) {
                if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    folder += Path.DirectorySeparatorChar;
                folder += subFolder;
                DirectoryInfo dirInfo = new DirectoryInfo(folder);
                return ((!dirInfo.Exists) ? System.IO.Directory.CreateDirectory(folder) : dirInfo);
            }

            return null;
        }

        public static DirectoryInfo GetFolderInfo(string root, string subFolder, string separator = null) {
            if (string.IsNullOrEmpty(separator))
                separator = Path.DirectorySeparatorChar.ToString();
            
            string folder = string.IsNullOrEmpty(root) ? "" : root;
            if (!string.IsNullOrEmpty(folder)) {
                if (!folder.EndsWith(separator))
                    folder += separator;
                folder += subFolder;
                DirectoryInfo dirInfo = new DirectoryInfo(folder);
                if (!dirInfo.Exists) {
                    throw new DirectoryNotFoundException(string.Format("The {0} folder not found.", folder));
                }

                return dirInfo;
            }

            return null;
        }

        public static string GetLastFolder(string folder) {
            int idx = folder.LastIndexOf(Path.DirectorySeparatorChar);
            return ((idx != -1) ? folder.Substring(idx + 1) : folder);

        }
    }
}