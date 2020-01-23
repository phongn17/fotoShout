using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Db {
    [ServiceContract(Name = "DbClientSyncService", Namespace = "http://fotoshout.com.synchronization", SessionMode = SessionMode.Required)]
    public interface IDbClientSyncService : IDbSyncService {
        [OperationContract(IsInitiating = true)]
        void SetServerScopeDescription(DbSyncScopeDescription scopeDescription);

        [OperationContract]
        void SetSyncDirection(SyncDirectionOrder syncDirection);

        [OperationContract]
        [FaultContract(typeof(WebSyncFaultException))]
        void CreateScopeDescription(DbSyncScopeDescription scopeDescription);

        [OperationContract]
        [FaultContract(typeof(WebSyncFaultException))]
        void GenerateSnapshot(string destFileName);

        [OperationContract]
        [FaultContract(typeof(WebSyncFaultException))]
        bool NeedsScope();
    }
}
