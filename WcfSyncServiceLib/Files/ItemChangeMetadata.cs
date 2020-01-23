using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Files {
    [DataContract]
    public class ItemChangeMetadata {
        public ItemChangeMetadata(SyncId itemId, ChangeKind changeKind, SyncVersion creationVersion, SyncVersion changeVersion) {
            ItemId = itemId;
            ChangeKind = changeKind;
            CreationVersion = creationVersion;
            ChangeVersion = changeVersion;
        }

        [DataMember]
        public SyncId ItemId;
        [DataMember]
        public ChangeKind ChangeKind;
        [DataMember]
        public SyncVersion CreationVersion;
        [DataMember]
        public SyncVersion ChangeVersion;
    }
}
