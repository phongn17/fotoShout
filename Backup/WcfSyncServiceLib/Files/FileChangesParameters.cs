using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FsSyncWebService.Files {
    [DataContract]
    public class FileChangesParameters {
        [DataMember]
        public object DataRetriever;

        [DataMember]
        public ChangeBatch ChangeBatch { get; set; }
    }
}
