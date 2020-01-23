using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace FsSyncWebService.Files {
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IFileServerSyncService : IFileSyncService {
        [OperationContract]
        [FaultContract(typeof(WebSyncFaultException))]
        Guid GetReplicaId();
    }
}
