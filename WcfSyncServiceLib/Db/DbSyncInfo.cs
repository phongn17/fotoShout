using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Db {
    [DataContract]
    public class DbSyncInfo: SyncInfo {
        public DbSyncInfo(string syncAction, string scopeName, string connection, string[] staticTables, string[] dynamicTables, bool checkAndCreate = false) 
            : base(syncAction) {
            SyncScopeName = scopeName;
            Connection = connection;
            StaticSyncTables = staticTables;
            DynamicSyncTables = dynamicTables;
            CheckAndCreate = checkAndCreate;
        }

        [DataMember]
        public string SyncScopeName { get; set; }

        [DataMember]
        public string Connection { get; set; }
        [DataMember]
        public string[] StaticSyncTables { get; set; }
        [DataMember]
        public string[] DynamicSyncTables { get; set; }

        public bool CheckAndCreate { get; set; }
    }
}
