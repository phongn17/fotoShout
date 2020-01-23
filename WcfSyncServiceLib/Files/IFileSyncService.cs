using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Files {
    [ServiceContract(Name = "FsSyncWebService.Files.FileSyncService", Namespace = "http://fotoshout.com.synchronization", SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(SyncIdFormatGroup))]
    [ServiceKnownType(typeof(IFileDataRetriever))]
    [ServiceKnownType(typeof(WebSyncFaultException))]
    [ServiceKnownType(typeof(SyncBatchParameters))]
    [ServiceKnownType(typeof(FileChangesParameters))]
    public interface IFileSyncService {
        [OperationContract(IsInitiating = true)]
        void Initialize(FileSyncInfo syncInfo);

        [OperationContract(IsInitiating = true)]
        string Ping();

        [OperationContract]
        void BeginSession(SyncProviderPosition position, byte[] changeApplierInfo);

        [OperationContract]
        SyncBatchParameters GetKnowledge();

        [OperationContract]
        FileChangesParameters GetChanges(uint batchSize, SyncKnowledge destKnowledge);

        [OperationContract]
        SyncSessionStatistics ApplyChanges(ConflictResolutionPolicy resolutionPolicy, ChangeBatch sourceChanges, object changeData);

        [OperationContract]
        void EndSession(byte[] changeApplierInfo);

        [OperationContract]
        SyncIdFormatGroup GetIdFormats();

        [OperationContract(IsTerminating = true)]
        void Cleanup();
    }
}
