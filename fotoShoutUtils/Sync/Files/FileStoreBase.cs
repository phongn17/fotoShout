using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Files {
    public abstract class FileStoreBase : KnowledgeSyncProvider, INotifyingChangeApplierTarget {
        private SyncProviderPosition _position;
        
        public FileStoreBase(string path) {
            Path = path;
            RequestedBatchSize = 1;
        }
        
        public string Path { get; protected set; }

        public uint RequestedBatchSize { get; set; }

        public string Name {
            get {
                return Path;
            }
        }

        protected SyncSessionContext SyncSessionContext { get; set; }
        
        #region // KnowledgeSyncProvider Implementations
        
        public override void BeginSession(SyncProviderPosition position, SyncSessionContext syncSessionContext) {
            _position = position;
            SyncSessionContext = syncSessionContext;
        }

        public override FullEnumerationChangeBatch GetFullEnumerationChangeBatch(uint batchSize, SyncId lowerEnumerationBound, SyncKnowledge knowledgeForDataRetrieval, out object changeDataRetriever) {
            throw new NotImplementedException("Not implemented");
        }

        public override void ProcessFullEnumerationChangeBatch(ConflictResolutionPolicy resolutionPolicy, FullEnumerationChangeBatch sourceChanges, object changeDataRetriever, SyncCallbacks syncCallbacks, SyncSessionStatistics sessionStatistics) {
            throw new NotImplementedException("Not Implemented");
        }

        protected virtual void BeginSession() {
        }
        
        #endregion // KnowledgeSyncProvider Implementations

        #region INotifyingChangeApplierTarget Implementations

        public abstract void SaveItemChange(SaveChangeAction saveChangeAction, ItemChange change, SaveChangeContext context);

        public void SaveChangeWithChangeUnits(ItemChange change, SaveChangeWithChangeUnitsContext context) {
            throw new NotImplementedException("Not Implemented");
        }

        public void SaveConflict(ItemChange conflictingChange, object conflictingChangeData, SyncKnowledge conflictingChangeKnowledge) {
            throw new NotImplementedException("Not Implemented");
        }

        public bool TryGetDestinationVersion(ItemChange sourceChange, out ItemChange destinationVersion) {
            throw new NotImplementedException("Not Implemented");
        }

        public abstract IChangeDataRetriever GetDataRetriever();
        public abstract ulong GetNextTickCount();
        public abstract void StoreKnowledgeForScope(SyncKnowledge knowledge, ForgottenKnowledge forgottenKnowledge);

        #endregion // INotifyingChangeApplierTarget Implementations

    }
}
