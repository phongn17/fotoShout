using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace FotoShoutUtils.Utils.IO {
    public class FileUtils {
        
        public static string[] GetFileNames(string folder, string exts) {
            return GetFileNames(folder, exts.Split('|'));
        }

        public static string GetFirstFileName(string folder, string exts) {
            return GetFirstFileName(folder, exts.Split('|'));
        }

        public static string[] GetFileNames(string folder, string[] exts) {
            if (string.IsNullOrEmpty(folder))
                return null;

            DirectoryInfo di = new DirectoryInfo(folder);
            if (!di.Exists)
                throw new DirectoryNotFoundException(string.Format("The {0} folder not found.", folder));

            HashSet<string> fileNames = new HashSet<string>();
            foreach (string ext in exts) {
                FileInfo[] files = di.GetFiles(ext);
                foreach (FileInfo file in files) {
                    fileNames.Add(file.Name);
                }
            }

            return fileNames.ToArray();
        }

        public static string GetFirstFileName(string folder, string[] exts) {
            if (string.IsNullOrEmpty(folder))
                return "";

            DirectoryInfo di = new DirectoryInfo(folder);
            if (!di.Exists)
                throw new DirectoryNotFoundException(string.Format("The {0} folder not found.", folder));

            foreach (string ext in exts) {
                FileInfo[] files = di.GetFiles(ext);
                if (files.Length > 0)
                    return files[0].Name;
            }

            return "";
        }

        public static string GetFilenameWithoutExt(string filename) {
            int idx = filename.LastIndexOf(".");
            return ((idx != -1) ? filename.Substring(0, idx) : filename);

        }

        public static string GetContentType(string id, bool isExt = false) {
            string ext = (isExt ? id : Path.GetExtension(id)).ToLower();
            string contentType = "application/octetstream";
            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (registryKey != null && registryKey.GetValue("Content Type") != null)
                contentType = registryKey.GetValue("Content Type").ToString();
            return contentType;
        }
    }
}