using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using FotoShoutApi.Utils;
using FotoShoutApi.Security;
using System.DirectoryServices;

namespace FotoShoutApi.Services.IO {
    public class DirectoryService {
        public const string ADDS_PROTOCOL_IIS = "IIS://";
        public const string ADDS_PROTOCOL_IIS_ROOT = "IIS://localhost/";
        public const string DIRECTORY_IIS_SERVICE = "W3SVC/1/Root";
        
        public const string ADDS_NODETYPE_SERVICE = "Service";
        public const string ADDS_NODETYPE_SERVER = "Server";
        public const string ADDS_NODETYPE_COMPUTER = "Computer";
        public const string ADDS_NODETYPE_VIRTUALDIR = "VirtualDir";

        public const string ADDS_SCHEMA_IISWEBVIRTUALDIR = "IIsWebVirtualDir";

        public static string GenerateVirtualPath(string physicalPath, string vRoot = null) {
            if (string.IsNullOrEmpty(physicalPath))
                return "";

            if (string.IsNullOrEmpty(vRoot))
                vRoot = AppConfigs.VirtualRoot;

            string vDirName = Cryptography.GenerateMD5Hash(physicalPath);
            string metabasePath = DirectoryService.ADDS_PROTOCOL_IIS_ROOT + DirectoryService.DIRECTORY_IIS_SERVICE + DirectoryService.GetVirtualRelativePath(vRoot);
            CreateVirtualDirectoryIfAny(metabasePath, vDirName, physicalPath);

            return vDirName;
        }

        private static string GetVirtualRelativePath(string vRoot) {
            int idx = vRoot.IndexOf("http://");
            if (idx != 0 || vRoot.Length == "http://".Length)
                throw new InvalidArgumentException("virtual directory root");

            idx = vRoot.IndexOf("/", "http://".Length);
            if (idx == -1 || idx == (vRoot.Length - 1))
                return "";

            return vRoot.Substring(idx);
        }
        
        private static void CreateVirtualDirectoryIfAny(string metabasePath, string vDirName, string physicalPath) {
            try {
                DirectoryEntry service = new DirectoryEntry(metabasePath);
                
                // Check if the virtual directory already exists
                DirectoryEntry matchingVDir = service.Children.Cast<DirectoryEntry>().Where(v => v.Name == vDirName).SingleOrDefault();
                if (matchingVDir != null)
                    return;
                
                string scName = service.SchemaClassName;
                if (scName.EndsWith(DirectoryService.ADDS_NODETYPE_SERVER) || scName.EndsWith(DirectoryService.ADDS_NODETYPE_COMPUTER) || scName.EndsWith(DirectoryService.ADDS_NODETYPE_VIRTUALDIR)) {
                    DirectoryEntries vDirs = service.Children;
                    DirectoryEntry vDir = vDirs.Add(vDirName, DirectoryService.ADDS_SCHEMA_IISWEBVIRTUALDIR);
                    vDir.Properties["Path"][0] = physicalPath;
                    vDir.Properties["AppFriendlyName"][0] = vDirName;
                    vDir.Properties["EnableDirBrowsing"][0] = false;
                    vDir.Properties["AccessRead"][0] = true;
                    vDir.Properties["AccessWrite"][0] = false;
                    vDir.Properties["AccessScript"][0] = true;
                    vDir.Properties["AppIsolated"][0] = "1";
                    vDir.Properties["AppRoot"][0] = "/LM" + metabasePath.Substring(metabasePath.IndexOf("/", DirectoryService.ADDS_PROTOCOL_IIS.Length));
                    vDir.CommitChanges();
                    vDir.Invoke("AppCreate", 1);
                }
            }
            catch (Exception ex) {
                throw new DsUpdateException(ex.Message + "\nmetabasePath: " + metabasePath + ", Virtual Dir: " + vDirName + ", physical Path: " + physicalPath, ex);
            }
        }

    }
}