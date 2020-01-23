using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FsSyncWebService.Db {
    [DataContract]
    [KnownType(typeof(DataSet))]
    public class DbChangesParameters {
        [DataMember]
        public object DataRetriever;

        [DataMember]
        public ChangeBatch ChangeBatch { get; set; }
    }
}
