using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FsSyncWebService {
    [DataContract]
    public class WebSyncFaultException {
        public WebSyncFaultException(string message, Exception innerException) {
            Message = message;
            InnerException = innerException;
        }
        
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public Exception InnerException { get; set; }
    }
}
