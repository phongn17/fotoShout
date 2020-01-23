using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Files {
    public abstract class FileSyncService: IFileSyncService{
        private SyncIdFormatGroup _idFormats = null;
        private SyncSessionContext _sessionContext = null;

        public UnmanagedSyncProviderWrapper Provider { get; set; }

        public SyncSessionContext SessionContext {
            get {
                if (_sessionContext == null)
                    _sessionContext = new SyncSessionContext(GetIdFormats(), null);

                return _sessionContext;
            }
            set {
                _sessionContext = value;
            }
        }

        public SyncIdFormatGroup GetIdFormats() {
            if (_idFormats == null) {
                _idFormats = new SyncIdFormatGroup();
                // 4 byte change unit id
                _idFormats.ChangeUnitIdFormat.IsVariableLength = false;
                _idFormats.ChangeUnitIdFormat.Length = 4;
                // Set format of replica id for using Guid ids
                _idFormats.ReplicaIdFormat.IsVariableLength = false;
                _idFormats.ReplicaIdFormat.Length = 16;
                // Set format of item id for using global id
                _idFormats.ItemIdFormat.IsVariableLength = false;
                _idFormats.ItemIdFormat.Length = 24;
            }

            return _idFormats;
        }

        public void Initialize(FileSyncInfo syncInfo) {
            try {
                Configure(syncInfo);
            }
            catch (Exception ex) {
                throw new FaultException<WebSyncFaultException>(new WebSyncFaultException(string.Format("Unable to initialize a file provider at {0}.", syncInfo.ReplicaRootPath), ex));
            }
        }

        public void BeginSession(Microsoft.Synchronization.SyncProviderPosition position, byte[] changeApplierInfo) {
            SyncSessionContext sessionContext = SessionContext;
            sessionContext.ChangeApplierInfo = changeApplierInfo;
            Provider.BeginSession(position, sessionContext);
        }

        public SyncBatchParameters GetKnowledge() {
            SyncBatchParameters destParamaters = new SyncBatchParameters();
            Provider.GetSyncBatchParameters(out destParamaters.BatchSize, out destParamaters.DestinationKnowledge);
            return destParamaters;
        }

        public FileChangesParameters GetChanges(uint batchSize, Microsoft.Synchronization.SyncKnowledge destKnowledge) {
            try {
                FileChangesParameters changesParams = new FileChangesParameters();
                changesParams.ChangeBatch = Provider.GetChangeBatch(batchSize, destKnowledge, out changesParams.DataRetriever);
                return changesParams;
            }
            catch (Exception ex) {
                throw new FaultException<WebSyncFaultException>(new WebSyncFaultException("Unable to get change batch.", ex));
            }
        }

        public SyncSessionStatistics ApplyChanges(ConflictResolutionPolicy resolutionPolicy, ChangeBatch sourceChanges, object changeData) {
            SyncSessionStatistics sessionStatistics = new SyncSessionStatistics();
            Provider.ProcessChangeBatch(resolutionPolicy, sourceChanges, changeData, new SyncCallbacks(), sessionStatistics);
            return sessionStatistics;
        }

        public void EndSession(byte[] changeApplierInfo) {
            SyncSessionContext sessionContext = SessionContext;
            sessionContext.ChangeApplierInfo = changeApplierInfo;
            Provider.EndSession(sessionContext);
        }

        public virtual void Cleanup() {
            SessionContext = null;
            Provider = null;
        }
        
        public abstract string Ping();

        protected abstract void Configure(FileSyncInfo syncInfo);
    }
}
