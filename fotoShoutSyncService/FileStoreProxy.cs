using FotoShoutUtils.Sync.Files;
using FsSyncWebService;
using FsSyncWebService.Files;
using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutSyncService {
    public class FileStoreProxy : FileStoreBase, IChangeDataRetriever, INotifyingChangeApplierTarget, IDisposable {
        private string[] _filters = null;
        private SyncKnowledge _syncKnowledge = null;
        private ForgottenKnowledge _forgottenKnowledge = null;

        protected bool disposed = false;

        protected string ServiceUri { get; set; }
        protected IRemoteFileSyncService Proxy { get; set; }

        public FileStoreProxy(string path, string serviceUri, string[] filters)
            : base(path) {
                _filters = filters;
            ServiceUri = serviceUri;
            Proxy = CreateProxy();
            Proxy.Initialize();
        }

        public SyncKnowledge SyncKnowledge {
            get {
                return _syncKnowledge;
            }
            set {
                _syncKnowledge = value;
            }
        }

        public ForgottenKnowledge ForgottenKnowledge {
            get {
                return _forgottenKnowledge;
            }
            set {
                _forgottenKnowledge = value;
            }
        }

        public string Ping() {
            return Proxy.Ping();
        }

        #region KnowledgeSyncProvider Implementations

        public override void EndSession(SyncSessionContext syncSessionContext) {
        }

        public override void GetSyncBatchParameters(out uint batchSize, out SyncKnowledge knowledge) {
            batchSize = RequestedBatchSize;
            try {
                SyncKnowledge = Proxy.GetCurrentSyncKnowledge(Path, _filters);
            }
            catch (Exception ex) {
                throw ex;
            }

            knowledge = SyncKnowledge.Clone();
        }

        public override SyncIdFormatGroup IdFormats {
            get {
                return SyncDetails.GetIdFormats();
            }
        }

        public override ChangeBatch GetChangeBatch(uint batchSize, SyncKnowledge destinationKnowledge, out object changeDataRetriever) {
            throw new NotImplementedException("Not implemented");
        }

        public override void ProcessChangeBatch(ConflictResolutionPolicy resolutionPolicy, ChangeBatch sourceChanges, object changeDataRetriever, SyncCallbacks syncCallbacks, SyncSessionStatistics sessionStatistics) {
            ItemsChangeInfo localVersions = null;
            try {
                localVersions = Proxy.GetChanges(Path, sourceChanges, _filters);
            }
            catch (Exception ex) {
                throw ex;
            }

            // Now we call the change applier
            // The change applier will compare the local and remote versions, apply 
            // non-conflicting changes, and will also detect conflicts and react as specified
            ForgottenKnowledge = new ForgottenKnowledge(IdFormats, SyncKnowledge);
            NotifyingChangeApplier changeApplier = new NotifyingChangeApplier(IdFormats);
            changeApplier.ApplyChanges(resolutionPolicy, sourceChanges, changeDataRetriever as IChangeDataRetriever, RemoteSyncDetails.GenerateChanges(localVersions),
                SyncKnowledge.Clone(), ForgottenKnowledge, this, SyncSessionContext, syncCallbacks);
        }
        #endregion // KnowledgeSyncProvider Implementations

        #region IChangeDataRetriever Implementations

        public object LoadChangeData(LoadChangeContext loadChangeContext) {
            return null;
        }

        #endregion // IChangeDataRetriever Implementations

        #region INotifyingChangeApplierTarget Implementations

        public override IChangeDataRetriever GetDataRetriever() {
            throw new NotImplementedException("Not Implemented");
        }

        public override ulong GetNextTickCount() {
            return 0;
        }


        public override void SaveItemChange(SaveChangeAction saveChangeAction, ItemChange change, SaveChangeContext context) {
            // Extract the data from the change
            TransferMechanism data = context.ChangeData as TransferMechanism;

            // Now apply the change 
            if (!(saveChangeAction == SaveChangeAction.UpdateVersionOnly) && ((change.ChangeKind & ChangeKind.Deleted) == 0)) {
                ItemMetadata item = new ItemMetadata();
                item.ItemId = change.ItemId;
                item.ChangeVersion = change.ChangeVersion;
                item.CreationVersion = change.CreationVersion;
                item.Uri = data.Uri;

                RemoteFileInfo fileInfo = new RemoteFileInfo();
                fileInfo.FolderPath = Path;
                fileInfo.Length = data.DataStream.Length;
                fileInfo.Metadata = item;
                fileInfo.FileByteStream = data.DataStream;
                Proxy.UploadFile(fileInfo);

                // Close input stream
                data.DataStream.Close();
            }
            else {
                Proxy.DeleteFile(Path, change.ItemId, _filters);
            }

            // If we made it here, the change was successfully applied locally
            // (or it is a version only change), so we can update our knowledge with the 
            // learned knowledge from the change
            context.GetUpdatedDestinationKnowledge(out _syncKnowledge, out _forgottenKnowledge);
        }

        public override void StoreKnowledgeForScope(SyncKnowledge knowledge, ForgottenKnowledge forgottenKnowledge) {
            try {
                Proxy.StoreKnowledgeForScope(Path, knowledge, forgottenKnowledge, _filters);
            }
            catch (Exception exc) {
                throw exc;
            }
        }
        
        #endregion // INotifyingChangeApplierTarget Implementations

        ~FileStoreProxy() {
            Dispose(false);
        }

        protected virtual IRemoteFileSyncService CreateProxy() {
            ChannelFactory<IRemoteFileSyncService> factory = new ChannelFactory<IRemoteFileSyncService>(ServiceUri);
            IRemoteFileSyncService proxy = factory.CreateChannel();
            //IRemoteFileSyncService proxy = new RemoteFileSyncService();

            return proxy;
        }

        public virtual void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void CloseProxy() {
            if (Proxy != null) {
                Proxy.Cleanup();
                ICommunicationObject channel = Proxy as ICommunicationObject;
                if (channel != null) {
                    try {
                        channel.Close();
                    }
                    catch (TimeoutException) {
                        channel.Abort();
                    }
                    catch (CommunicationException) {
                        channel.Abort();
                    }
                }

                Proxy = null;
            }
        }
        
        private void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    if (Proxy != null)
                        CloseProxy();
                }
                disposed = true;
            }
        }
        
        private Message CreateMessage(string method, object body) {
            string action = string.Format("http://fotoshout.com.synchronization/IRemoteFileSyncService/{0}", method);
            return Message.CreateMessage(MessageVersion.Soap11, action, body);
        }

        private Message CreateMessage(string method, object body, DataContractSerializer ser) {
            string action = string.Format("http://fotoshout.com.synchronization/IRemoteFileSyncService/{0}", method);
            return Message.CreateMessage(MessageVersion.Soap11, action, body, ser);
        }
    }
}
