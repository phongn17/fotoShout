using FotoShoutUtils.Sync.Db;
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
    public abstract class DbSyncService: IDbSyncService {
        //protected bool isProxyToCompactDb = false;
        protected DirectoryInfo sessionBatchingDirectory = null;
        protected Dictionary<string, string> batchIdToFileMapper = null;

        //private int batchCount = 0;

        public void Initialize(DbSyncInfo syncInfo) {
            Configure(syncInfo);
            batchIdToFileMapper = new Dictionary<string, string>();
        }

        public RelationalSyncProvider Provider { get; set; }

        public void BeginSession(Microsoft.Synchronization.SyncProviderPosition position) {
            batchIdToFileMapper = new Dictionary<string, string>();
            Provider.BeginSession(position, null/* SyncSessionContext */);
            //batchCount = 0;
        }

        public SyncBatchParameters GetKnowledge() {
            SyncBatchParameters destParamaters = new SyncBatchParameters();
            Provider.GetSyncBatchParameters(out destParamaters.BatchSize, out destParamaters.DestinationKnowledge);
            return destParamaters;
        }

        public DbChangesParameters GetChanges(uint batchSize, Microsoft.Synchronization.SyncKnowledge destKnowledge) {
            DbChangesParameters changesParams = new DbChangesParameters();
            changesParams.ChangeBatch = Provider.GetChangeBatch(batchSize, destKnowledge, out changesParams.DataRetriever);

            DbSyncContext context = changesParams.DataRetriever as DbSyncContext;
            // Check to see if data is batched
            if (context != null && context.IsDataBatched) {
                // Don't send the file location info, just send the file
                string filename = new FileInfo(context.BatchFileName).Name;
                batchIdToFileMapper[filename] = context.BatchFileName;
                context.BatchFileName = filename;
            }

            return changesParams;
        }

        public SyncSessionStatistics ApplyChanges(ConflictResolutionPolicy resolutionPolicy, ChangeBatch sourceChanges, object changeData) {
            DbSyncContext context = changeData as DbSyncContext;
            // Check to see if data is batched
            if (context != null && context.IsDataBatched) {
                string remoteId = context.MadeWithKnowledge.ReplicaId.ToString();
                // Data is batched. The client should have uploaded this file to us prior to calling ApplyChanges, so look for it
                // The id would be the DbSyncContext.BatchFileName which is just the batch filename without the complete path
                string localBatchFilename = null;
                if (!batchIdToFileMapper.TryGetValue(context.BatchFileName, out localBatchFilename)) // Service did not received this file
                    throw new FaultException<WebSyncFaultException>(new WebSyncFaultException(string.Format("No batch file uploaded for the id {0}.", context.BatchFileName), null));
                context.BatchFileName = localBatchFilename;
            }

            SyncSessionStatistics sessionStatistics = new SyncSessionStatistics();
            Provider.ProcessChangeBatch(resolutionPolicy, sourceChanges, changeData, new SyncCallbacks(), sessionStatistics);
            return sessionStatistics;
        }

        public bool HasUploadedBatchFile(string batchFileId, string remoteId) {
            CheckAndCreateBatchingDirectory(remoteId);

            // The batchFileId is the file name without the path
            FileInfo fileInfo = new FileInfo(Path.Combine(sessionBatchingDirectory.FullName, batchFileId));
            if (fileInfo.Exists && !batchIdToFileMapper.ContainsKey(batchFileId)) // if file exists but not in the mapping, then add it
                batchIdToFileMapper.Add(batchFileId, fileInfo.FullName);

            return fileInfo.Exists;
        }

        public void UploadBatchFile(string batchFileId, byte[] batchContent, string remoteId) {
            try {
                if (HasUploadedBatchFile(batchFileId, remoteId)) // Service has already received this file, so donnot save it again
                    return;

                string localFileLocation = Path.Combine(sessionBatchingDirectory.FullName, batchFileId);
                using (Stream fileStream = new FileStream(localFileLocation, FileMode.Create, FileAccess.Write)) {
                    fileStream.Write(batchContent, 0, batchContent.Length);
                }
                // Save this id to file location mapping
                batchIdToFileMapper[batchFileId] = localFileLocation;
            }
            catch (Exception ex) {
                throw new FaultException<WebSyncFaultException>(new WebSyncFaultException(string.Format("Unable to save batch file {0}.", batchFileId), ex));
            }
        }

        public byte[] DownloadBatchFile(string batchFileId) {
            try {
                string localBatchFilename = null;
                if (!batchIdToFileMapper.TryGetValue(batchFileId, out localBatchFilename))
                    throw new FaultException<WebSyncFaultException>(new WebSyncFaultException(string.Format("Unbale to retrieve batch file for the id {0}.", batchFileId), null));
                using (Stream localFileStream = new FileStream(localBatchFilename, FileMode.Open, FileAccess.Read)) {
                    byte[] content = new byte[localFileStream.Length];
                    localFileStream.Read(content, 0, content.Length);
                    return content;
                }
            }
            catch (Exception ex) {
                throw new FaultException<WebSyncFaultException>(new WebSyncFaultException(string.Format("Unable to read batch file for the id {0}.", batchFileId), ex));
            }
        }

        public void EndSession() {
            Provider.EndSession(null);
        }

        public void Cleanup() {
            Provider = null;

            // Delete all files in the temp session directory
            if (sessionBatchingDirectory != null) {
                sessionBatchingDirectory.Refresh();
                if (sessionBatchingDirectory.Exists) {
                    try {
                        sessionBatchingDirectory.Delete(true);
                    }
                    catch {
                        /// TODO: provide a log entry
                    }
                }
            }
        }

        public abstract string Ping();
        
        protected abstract void Configure(DbSyncInfo syncInfo);

        private void CheckAndCreateBatchingDirectory(string remoteId) {
            // Check if we have the temp directory for this session
            if (sessionBatchingDirectory == null) {
                // Generate a unique id for the directory. We use the remote id of the store enumerating the changes so that the local temp directory
                // is same for a gicen source across sync sessions. This enables us to restart a failed sync by not downloading already received files
                string sessionDir = Path.Combine(Provider.BatchingDirectory, "WebSync_" + remoteId);
                sessionBatchingDirectory = new DirectoryInfo(sessionDir);
                if (!sessionBatchingDirectory.Exists)
                    sessionBatchingDirectory.Create();
            }
        }

    }
}
