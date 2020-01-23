using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Files {
    public class FileServerSyncProviderProxy: KnowledgeSyncProvider, IDisposable {
        private const int READERQUOTAS_MAXARRAY = 10485760;
        private const int MESSAGESIZE_MAX = 10485760;

        protected bool disposed = false;

        public FileServerSyncProviderProxy(string syncAction, string replicaRootPath, string[] filters, FileSyncOptions syncOptions, string serviceUri, string syncClass = null) {
            ServiceUri = serviceUri;
            Proxy = CreateProxy();
            try {
                FileSyncInfo syncInfo = new FileSyncInfo(syncAction, replicaRootPath, filters, syncOptions);
                if (!string.IsNullOrEmpty(syncClass))
                    syncInfo.ServerSyncClass = syncClass;
                Proxy.Initialize(syncInfo);
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        protected string ServiceUri { get; set; }
        protected IFileSyncService Proxy { get; set; }

        public string Ping() {
            return Proxy.Ping();
        }

        public override void BeginSession(SyncProviderPosition position, SyncSessionContext syncSessionContext) {
            Proxy.BeginSession(position, syncSessionContext.ChangeApplierInfo);
        }

        public override void EndSession(SyncSessionContext syncSessionContext) {
            Proxy.EndSession(syncSessionContext.ChangeApplierInfo);
        }

        public override ChangeBatch GetChangeBatch(uint batchSize, SyncKnowledge destKnowledge, out object changeDataRetriever) {
            FileChangesParameters changesParams = Proxy.GetChanges(batchSize, destKnowledge);
            changeDataRetriever = changesParams.DataRetriever;

            return changesParams.ChangeBatch;
        }

        public override FullEnumerationChangeBatch GetFullEnumerationChangeBatch(uint batchSize, SyncId lowerEnumerationBound, SyncKnowledge knowledgeForDataRetrieval, out object changeDataRetriever) {
            throw new NotImplementedException();
        }

        public override void GetSyncBatchParameters(out uint batchSize, out SyncKnowledge knowledge) {
            SyncBatchParameters batchParams = Proxy.GetKnowledge();
            batchSize = batchParams.BatchSize;
            knowledge = batchParams.DestinationKnowledge;
        }

        public override SyncIdFormatGroup IdFormats {
            get {
                return Proxy.GetIdFormats();
            }
        }

        public override void ProcessChangeBatch(ConflictResolutionPolicy resolutionPolicy, ChangeBatch sourceChanges, object changeDataRetriever, SyncCallbacks syncCallbacks, SyncSessionStatistics sessionStatistics) {
            SyncSessionStatistics stats = Proxy.ApplyChanges(resolutionPolicy, sourceChanges, changeDataRetriever);
            sessionStatistics.ChangesApplied += stats.ChangesApplied;
            sessionStatistics.ChangesFailed += stats.ChangesFailed;
        }

        public override void ProcessFullEnumerationChangeBatch(ConflictResolutionPolicy resolutionPolicy, FullEnumerationChangeBatch sourceChanges, object changeDataRetriever, SyncCallbacks syncCallbacks, SyncSessionStatistics sessionStatistics) {
            throw new NotImplementedException();
        }
        
        ~FileServerSyncProviderProxy() {
            Dispose(false);
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

        protected virtual IFileSyncService CreateProxy() {
            WSHttpBinding binding = new WSHttpBinding();
            binding.Security.Mode = SecurityMode.None;
            binding.ReliableSession.Enabled = true;
            binding.ReaderQuotas.MaxArrayLength = FileServerSyncProviderProxy.READERQUOTAS_MAXARRAY;
            binding.MaxReceivedMessageSize = FileServerSyncProviderProxy.MESSAGESIZE_MAX;
            ChannelFactory<IFileSyncService> factory = new ChannelFactory<IFileSyncService>(binding, new EndpointAddress(ServiceUri));
            IFileSyncService proxy = factory.CreateChannel();

            return proxy;
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
    }
}
