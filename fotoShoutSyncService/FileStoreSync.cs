using FotoShoutUtils.Sync.Files;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutSyncService {
    // A delegate type for hooking up item conflicting notifications
    public delegate void ItemConflictingEventHandler(object sender, ItemConflictingEventArgs ev);
    // A delegate type for hooking up notifications when an item constraint is violated
    public delegate void ItemConstraintEventHandler(object sender, ItemConstraintEventArgs ev);
    // A delegate type for hooking up synchronized notifications
    public delegate void SynchronizedEventHandler(object sender, FileSystemSynchronizedEventArgs ev);
    
    public class FileStoreSync : FileStore {
        public event ItemConflictingEventHandler ItemConflicting;
        public event ItemConstraintEventHandler ItemConstraint;
        public event SynchronizedEventHandler Synchronized;

        public FileStoreSync(string path, SyncDirectionOrder syncDirection, string[] filters) 
            : base(path, filters) {
            SyncDirection = syncDirection;
        }
        
        protected SyncDirectionOrder SyncDirection { get; private set; }

        public void Synchronize(KnowledgeSyncProvider remoteProvider, uint batchSize) {
            Synchronize(SyncDirection, this, remoteProvider, batchSize);
        }

        public void Synchronize(SyncDirectionOrder syncDirection, KnowledgeSyncProvider localProvider, KnowledgeSyncProvider remoteProvider, uint batchSize) {
            FileStoreSync fileStoreSync = localProvider as FileStoreSync;
            fileStoreSync.RequestedBatchSize = batchSize;
            ((FileStoreProxy)remoteProvider).RequestedBatchSize = batchSize;

            // Use sync-callbacks for conflicting items
            SyncCallbacks destCallbacks = localProvider.DestinationCallbacks;
            destCallbacks.ItemConflicting += new EventHandler<ItemConflictingEventArgs>(OnItemConflicting);
            destCallbacks.ItemConstraint += new EventHandler<ItemConstraintEventArgs>(OnItemConstraint);

            localProvider.Configuration.ConflictResolutionPolicy = ConflictResolutionPolicy.SourceWins;
            remoteProvider.Configuration.ConflictResolutionPolicy = ConflictResolutionPolicy.SourceWins;

            // Initiate orchestrator and sync
            SyncOrchestrator orchestrator = new SyncOrchestrator();
            orchestrator.LocalProvider = localProvider;
            orchestrator.RemoteProvider = remoteProvider;

            // Set sync direction
            orchestrator.Direction = syncDirection;
            // Execute the synchronize process
            SyncOperationStatistics syncStats = orchestrator.Synchronize();

            // Notify a synchronization took place
            FileSystemSynchronizedEventArgs ev = new FileSystemSynchronizedEventArgs(syncStats);
            OnSynchronized(orchestrator, ev);
        }
        
        protected virtual void OnItemConflicting(object sender, ItemConflictingEventArgs ev) {
            if (ItemConflicting != null)
                ItemConflicting(sender, ev);
        }

        protected virtual void OnItemConstraint(object sender, ItemConstraintEventArgs ev) {
            if (ItemConstraint != null)
                ItemConstraint(sender, ev);
        }

        protected virtual void OnSynchronized(object sender, FileSystemSynchronizedEventArgs ev) {
            if (Synchronized != null)
                Synchronized(sender, ev);
        }
    }
}
