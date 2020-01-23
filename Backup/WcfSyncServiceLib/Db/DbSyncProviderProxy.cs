using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Db {
    public abstract class DbSyncProviderProxy: KnowledgeSyncProvider, IDisposable {
        private string _batchingDirectory;
        private SyncIdFormatGroup _idFormats = null;

        protected bool disposed = false;

        public DbSyncProviderProxy(string syncAction, string scopeName, string connection, string[] staticTables, string[] dynamicTables, string syncClass, string serviceUri) {
            ServiceUri = serviceUri;
            Proxy = CreateProxy();
            try {
                DbSyncInfo syncInfo = new DbSyncInfo(syncAction, scopeName, connection, staticTables, dynamicTables);
                syncInfo.ServerSyncClass = syncClass;
                Proxy.Initialize(syncInfo);
            }
            catch (Exception ex) {
                throw ex;
            }

            BatchingDirectory = Environment.ExpandEnvironmentVariables("%TEMP%");
        }

        protected string ServiceUri { get; set; }
        protected IDbSyncService Proxy { get; set; }
        
        protected string BatchingDirectory {
            get {
                return _batchingDirectory;
            }
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("BatchingDirectory");
                try {
                    Uri uri = new Uri(value);
                    if (!uri.IsFile || uri.IsUnc)
                        throw new ArgumentException("Batching directory must be a local directory.");
                    _batchingDirectory = value;
                }
                catch (Exception ex) {
                    throw new ArgumentException("Invalid batching directory", ex);
                }
            }
        }

        protected DirectoryInfo LocalBatchingDirectory { get; set; }

        public string Ping() {
            return Proxy.Ping();
        }

        public override void BeginSession(SyncProviderPosition position, SyncSessionContext syncSessionContext) {
            Proxy.BeginSession(position);
        }

        public override void EndSession(SyncSessionContext syncSessionContext) {
            Proxy.EndSession();
            if (LocalBatchingDirectory != null) {
                LocalBatchingDirectory.Refresh();
                if (LocalBatchingDirectory.Exists)
                    LocalBatchingDirectory.Delete(true);
            }
        }

        public override ChangeBatch GetChangeBatch(uint batchSize, SyncKnowledge destKnowledge, out object changeDataRetriever) {
            DbChangesParameters changesParams = Proxy.GetChanges(batchSize, destKnowledge);
            changeDataRetriever = changesParams.DataRetriever;

            DbSyncContext context = changeDataRetriever as DbSyncContext;
            // Check if the data is batched
            if (context != null && context.IsDataBatched) {
                CheckAndCreateBatchingDirectory(context);
                DownloadBatchingFile(context);
            }

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
                if (_idFormats == null) {
                    _idFormats = new SyncIdFormatGroup();
                    // 1 byte change unit id (Harmonica default before flexible ids)
                    _idFormats.ChangeUnitIdFormat.IsVariableLength = false;
                    _idFormats.ChangeUnitIdFormat.Length = 1;
                    // Set format of replica id for using Guid ids
                    _idFormats.ReplicaIdFormat.IsVariableLength = false;
                    _idFormats.ReplicaIdFormat.Length = 16;
                    // Set format of item id for using global id
                    _idFormats.ItemIdFormat.IsVariableLength = true;
                    _idFormats.ItemIdFormat.Length = 10 * 1024;
                }

                return _idFormats;
            }
        }

        public override void ProcessChangeBatch(ConflictResolutionPolicy resolutionPolicy, ChangeBatch sourceChanges, object changeDataRetriever, SyncCallbacks syncCallbacks, SyncSessionStatistics sessionStatistics) {
            DbSyncContext context = changeDataRetriever as DbSyncContext;
            // Check if the data is batched
            if (context != null && context.IsDataBatched) {
                string filename = new FileInfo(context.BatchFileName).Name;
                // Retrieve the remote id from the MadeWithKnowledge.ReplicaId. MadeWithKnowledge is the local knowledge of the peer
                // that is enumerating the changes
                string remoteId = context.MadeWithKnowledge.ReplicaId.ToString();
                // Check if service already has this file
                if (!Proxy.HasUploadedBatchFile(filename, remoteId)) {
                    // Upload this file to remote service
                    byte[] content = null;
                    using (Stream stream = new FileStream(context.BatchFileName, FileMode.Open, FileAccess.Read)) {
                        content = new byte[stream.Length];
                        stream.Read(content, 0, content.Length);
                    }
                    if (content != null)
                        Proxy.UploadBatchFile(filename, content, remoteId);
                }

                context.BatchFileName = filename;
            }

            SyncSessionStatistics stats = Proxy.ApplyChanges(resolutionPolicy, sourceChanges, changeDataRetriever);
            sessionStatistics.ChangesApplied += stats.ChangesApplied;
            sessionStatistics.ChangesFailed += stats.ChangesFailed;
        }

        public override void ProcessFullEnumerationChangeBatch(ConflictResolutionPolicy resolutionPolicy, FullEnumerationChangeBatch sourceChanges, object changeDataRetriever, SyncCallbacks syncCallbacks, SyncSessionStatistics sessionStatistics) {
            throw new NotImplementedException();
        }

        ~DbSyncProviderProxy() {
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

        protected abstract IDbSyncService CreateProxy();

        private void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    if (Proxy != null)
                        CloseProxy();
                }
                disposed = true;
            }
        }

        private void DownloadBatchingFile(DbSyncContext context) {
            string localFilename = Path.Combine(LocalBatchingDirectory.FullName, context.BatchFileName);
            FileInfo localFileInfo = new FileInfo(localFilename);
            // Download the file if not yet exist
            if (!localFileInfo.Exists) {
                byte[] remoteFileContents = Proxy.DownloadBatchFile(context.BatchFileName);
                using (Stream localFileStream = new FileStream(localFilename, FileMode.Create, FileAccess.Write)) {
                    localFileStream.Write(remoteFileContents, 0, remoteFileContents.Length);
                }
            }
            // Set the batch file name of the context to the new local file name
            context.BatchFileName = localFilename;
        }

        private void CheckAndCreateBatchingDirectory(DbSyncContext context) {
            if (LocalBatchingDirectory == null) {
                // Retrieve the remote id from MadeWithKnowledge.ReplicaId. MadeWithKnowledge is the local knowledge of the peer
                // that is enumerating the changes
                string remoteId = context.MadeWithKnowledge.ReplicaId.ToString();

                // Generate a unique id for the directory. We use the remote id of the store enumerating the changes so that the local temp directory
                // is same for a gicen source across sync sessions. This enables us to restart a failed sync by not downloading already received files
                string batchDir = Path.Combine(BatchingDirectory, "WebSync_" + remoteId);
                LocalBatchingDirectory = new DirectoryInfo(batchDir);
                // Create the directory if not yet exist
                if (!LocalBatchingDirectory.Exists)
                    LocalBatchingDirectory.Create();
            }
        }

    }
}
