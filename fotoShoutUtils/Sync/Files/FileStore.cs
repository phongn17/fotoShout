using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Files {
    public class FileStore : FileStoreBase, IChangeDataRetriever {
        private SyncDetails _sync = null;
        private ForgottenKnowledge _forgottenKnowledge;

        public event AppliedChangeEventHandler AppliedChange;
        public event SkippedChangeEventHandler SkippedChange;

        public FileStore(string path, string[] filters) 
            : base(path) {
            _sync = new SyncDetails(path);
            SetScopeFilter(filters);
        }

        public void SetScopeFilter(string[] filters) {
            if (_sync == null)
                throw new NullReferenceException("Sync Details");
            _sync.SetScopeFilter(filters);
        }
        
        #region KnowledgeSyncProvider Implementations
        
        public override void EndSession(SyncSessionContext syncSessionContext) {
            EndSession();
        }

        public override ChangeBatch GetChangeBatch(uint batchSize, SyncKnowledge destinationKnowledge, out object changeDataRetriever) {
            return _sync.GetChangeBatch(batchSize, destinationKnowledge, out _forgottenKnowledge, out changeDataRetriever);
        }

        public override void GetSyncBatchParameters(out uint batchSize, out SyncKnowledge knowledge) {
            _sync.SetLocalTickCount();
            batchSize = RequestedBatchSize;
            knowledge = _sync.SyncKnowledge.Clone();
        }

        public override SyncIdFormatGroup IdFormats {
            get { return _sync.IdFormats; }
        }

        public override void ProcessChangeBatch(ConflictResolutionPolicy resolutionPolicy, ChangeBatch sourceChanges, object changeDataRetriever, SyncCallbacks syncCallbacks, SyncSessionStatistics sessionStatistics) {
            IEnumerable<ItemChange> localVersions = _sync.GetChanges(sourceChanges);

            // Now we call the change applier
            // The change applier will compare the local and remote versions, apply 
            // non-conflicting changes, and will also detect conflicts and react as specified
            ForgottenKnowledge destinationForgottenKnowledge = new ForgottenKnowledge(_sync.IdFormats, _sync.SyncKnowledge);
            NotifyingChangeApplier changeApplier = new NotifyingChangeApplier(_sync.IdFormats);

            changeApplier.ApplyChanges(resolutionPolicy, sourceChanges, changeDataRetriever as IChangeDataRetriever, localVersions,
                _sync.SyncKnowledge.Clone(), destinationForgottenKnowledge, this, SyncSessionContext, syncCallbacks);
        }

        private void EndSession() {
            _sync.Save();
        }

        #endregion // KnowledgeSyncProvider Implementations

        #region IChangeDataRetriever Implementations
        
        public object LoadChangeData(LoadChangeContext loadChangeContext) {
            return _sync.LoadChangeData(loadChangeContext);
        }
        
        #endregion // IChangeDataRetriever Implementations

        #region INotifyingChangeApplierTarget Implementations

        public override IChangeDataRetriever GetDataRetriever() {
            return _sync;
        }

        public override ulong GetNextTickCount() {
            return _sync.GetNextTickCount();
        }

        public override void SaveItemChange(SaveChangeAction saveChangeAction, ItemChange change, SaveChangeContext context) {
            // Extract the data from the change
            TransferMechanism data = context.ChangeData as TransferMechanism;
            // Item metadata
            ItemMetadata item = _sync.GetItemMetaData(saveChangeAction, change, data);

            // Now apply the change 
            if (!(saveChangeAction == SaveChangeAction.UpdateVersionOnly) && ((change.ChangeKind & ChangeKind.Deleted) == 0)) {
                // Only copy if destination file name is known
                // (May have been lost if loser of name collision)
                if (item.Uri != String.Empty) {
                    FileInfo fi = new FileInfo(System.IO.Path.Combine(Path, item.Uri));
                    if (!fi.Directory.Exists)
                        fi.Directory.Create();

                    // Change should have stream data, so copy the file data to the local store
                    using (FileStream outputStream = new FileStream(System.IO.Path.Combine(Path, item.Uri), FileMode.OpenOrCreate)) {
                        const int copyBlockSize = 4096;
                        byte[] buffer = new byte[copyBlockSize];

                        int bytesRead;
                        // Simple block-by-block copy
                        while ((bytesRead = data.DataStream.Read(buffer, 0, copyBlockSize)) > 0)
                            outputStream.Write(buffer, 0, bytesRead);
                        // Truncate if needed
                        outputStream.SetLength(outputStream.Position);
                    }

                    // Update the last write time from the updated file to the metadata entry
                    item.LastWriteTimeUtc = fi.LastWriteTimeUtc;
                }

                // Close input stream
                data.DataStream.Close();
            }
            else {
                // Not an update/create, so must be a delete or version only change
                // Is it a delete?...
                if (item.IsTombstone && item.Uri != String.Empty) {
                    // Change is a delete
                    File.Delete(System.IO.Path.Combine(Path, item.Uri));
                }
            }

            // If we made it here, the change was successfully applied locally
            // (or it is a version only change), so we can update our knowledge with the 
            // learned knowledge from the change
            _sync.GetUpdatedKnowledge(context);
        }

        public override void StoreKnowledgeForScope(SyncKnowledge knowledge, ForgottenKnowledge forgottenKnowledge) {
            // Update in memory knowledge
            _sync.SyncKnowledge = knowledge;
            _sync.ForgottenKnowledge = forgottenKnowledge;
        }

        #endregion INotifyingChangeApplier Target Implementations
        
        protected virtual void OnAppliedChange(object sender, AppliedChangeEventArgs ev) {
            if (AppliedChange != null)
                AppliedChange(sender, ev);
        }

        protected virtual void OnSkippedChange(object sender, SkippedChangeEventArgs ev) {
            if (SkippedChange != null)
                SkippedChange(sender, ev);
        }
    }
}
