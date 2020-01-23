using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService {
    [DataContract]
    public class SyncInfo {
        public SyncInfo(string syncAction) {
            SyncAction = syncAction;
        }

        [DataMember]
        public string SyncAction { get; set; }
        [DataMember]
        public string ServerSyncClass { get; set; }
    }
}
