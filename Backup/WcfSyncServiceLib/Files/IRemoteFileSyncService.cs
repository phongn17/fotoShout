using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Files {
    [ServiceContract(Namespace = "http://fotoshout.com.synchronization")]
    [ServiceKnownType(typeof(SyncIdFormatGroup))]
    [ServiceKnownType(typeof(ItemChange))]
    [ServiceKnownType(typeof(WebSyncFaultException))]
    [ServiceKnownType(typeof(SyncBatchParameters))]
    public interface IRemoteFileSyncService {
        [OperationContract]
        void Initialize();

        [OperationContract(IsInitiating = true)]
        string Ping();
        
        [OperationContract]
        SyncKnowledge GetCurrentSyncKnowledge(string folderPath, string[] filters);

        [OperationContract]
        ItemsChangeInfo GetChanges(string folderPath, ChangeBatch sourceChanges, string[] filters);

        [OperationContract]
        void UploadFile(RemoteFileInfo request);

        [OperationContract]
        void DeleteFile(string folderPath, SyncId itemID, string[] filters);

        [OperationContract]
        void StoreKnowledgeForScope(string folderPath, SyncKnowledge knowledge, ForgottenKnowledge forgotten, string[] filters);
        
        [OperationContract]
        void Cleanup();
    }
}
