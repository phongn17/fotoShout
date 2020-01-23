using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace FotoShoutApi.Utils.IO {
    public class PagingFileUtils {
        public static int GetNumFiles(string folder, string exts) {
            return GetNumFiles(folder, exts.Split('|'));
        }
        
        public static string[] GetFileNames(string folder, string exts, int page = 0, int pageSize = 12) {
            return GetFileNames(folder, exts.Split('|'), page, pageSize);
        }

        public static string GetFirstFileName(string folder, string exts) {
            return GetFirstFileName(folder, exts.Split('|'));
        }

        public static int GetNumFiles(string folder, string[] exts) {
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentNullException("Folder name is empty.");

            DirectoryInfo di = new DirectoryInfo(folder);
            if (!di.Exists)
                throw new DirectoryNotFoundException(string.Format("The {0} folder not found.", folder));

            int nFiles = 0;
            foreach (string ext in exts) {
                FileInfo[] files = di.GetFiles(ext);
                nFiles += files.Length;
            }

            return nFiles;
        }
        
        public static string[] GetFileNames(string folder, string[] exts, int page = 0, int pageSize = 12) {
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentNullException("Folder name is empty.");

            DirectoryInfo di = new DirectoryInfo(folder);
            if (!di.Exists)
                throw new DirectoryNotFoundException(string.Format("The {0} folder not found.", folder));

            HashSet<FileInfo> files = new HashSet<FileInfo>();
            foreach (string ext in exts) {
                IEnumerable<FileInfo> curfiles = di.GetFiles(ext);
                files.UnionWith(curfiles);
            }

            return (page > 0 && pageSize > 0 && (files.Count() > pageSize)) ?
                files.OrderBy(f => f.CreationTime).Skip((page - 1) * pageSize).Take(pageSize).Select(f => f.Name).ToArray() : 
                files.OrderBy(f => f.CreationTime).Select(f => f.Name).ToArray();
        }

        public static string GetFirstFileName(string folder, string[] exts) {
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentNullException("Folder name is empty.");

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

        public static object IOrderedEnumerable { get; set; }
    }
}