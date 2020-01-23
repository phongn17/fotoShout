using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Files {
    [DataContract]
    [KnownType(typeof(SyncId))]
    public class ItemsChangeInfo {
        [DataMember]
        public SyncIdFormatGroup IdFormats;
        [DataMember]
        public SyncId ReplicaId;
        [DataMember]
        public List<ItemChangeMetadata> ItemChanges;
    }
}
