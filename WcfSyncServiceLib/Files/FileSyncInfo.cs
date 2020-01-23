using Microsoft.Synchronization.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FsSyncWebService.Files {
    [DataContract]
    public class FileSyncInfo: SyncInfo {
        public const string FILESYNC_CLASS = "FotoShoutUtils.Sync.Files.FileServerSync, FotoShoutUtils";
        public FileSyncInfo(string syncAction, string replicaRootPath, string[] filters, FileSyncOptions syncOptions, string serverSyncClass = null)
            : base(syncAction) {
            ServerSyncClass = string.IsNullOrEmpty(serverSyncClass) ? FileSyncInfo.FILESYNC_CLASS : serverSyncClass;
            ReplicaRootPath = replicaRootPath;
            Filters = filters;
            SyncOptions = syncOptions;
        }

        [DataMember]
        public string ReplicaRootPath { get; set; }
        [DataMember]
        public string[] Filters { get; set; }
        [DataMember]
        public FileSyncOptions SyncOptions { get; set; }
    }
}
