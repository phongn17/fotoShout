using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Db {
    [ServiceContract(Name = "FsSyncWebService.DbSyncService", Namespace = "http://fotoshout.com.synchronization", SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(SyncIdFormatGroup))]
    [ServiceKnownType(typeof(DbSyncContext))]
    [ServiceKnownType(typeof(SyncSchema))]
    [ServiceKnownType(typeof(WebSyncFaultException))]
    [ServiceKnownType(typeof(SyncBatchParameters))]
    [ServiceKnownType(typeof(DbChangesParameters))]
    public interface IDbSyncService {
        [OperationContract(IsInitiating = true)]
        void Initialize(DbSyncInfo syncInfo);

        [OperationContract]
        string Ping();
        
        [OperationContract]
        void BeginSession(SyncProviderPosition position);

        [OperationContract]
        SyncBatchParameters GetKnowledge();

        [OperationContract]
        DbChangesParameters GetChanges(uint batchSize, SyncKnowledge destKnowledge);

        [OperationContract]
        SyncSessionStatistics ApplyChanges(ConflictResolutionPolicy resolutionPolicy, ChangeBatch sourceChanges, object changeData);

        [OperationContract]
        bool HasUploadedBatchFile(string batchFileId, string remoteId);

        [OperationContract]
        void UploadBatchFile(string batchFileId, byte[] batchFile, string remoteId);

        [OperationContract]
        byte[] DownloadBatchFile(string batchFileId);

        [OperationContract]
        void EndSession();

        [OperationContract(IsTerminating = true)]
        void Cleanup();
    }
}
