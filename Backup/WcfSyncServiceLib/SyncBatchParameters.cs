using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FsSyncWebService {
    [DataContract]
    public class SyncBatchParameters {
        [DataMember]
        public SyncKnowledge DestinationKnowledge;
        [DataMember]
        public uint BatchSize;
    }
}
