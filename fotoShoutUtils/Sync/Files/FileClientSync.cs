﻿using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Files {
    // A delegate type for hooking up change failed notifications
    public delegate void AppliedChangeEventHandler(object sender, AppliedChangeEventArgs ev);
    // A delegate type for hooking up change skipped notifications
    public delegate void SkippedChangeEventHandler(object sender, SkippedChangeEventArgs ev);
    // A delegate type for hooking up item conflicting notifications
    public delegate void ItemConflictingEventHandler(object sender, ItemConflictingEventArgs ev);
    // A delegate type for hooking up notifications when an item constraint is violated
    public delegate void ItemConstraintEventHandler(object sender, ItemConstraintEventArgs ev);
    // A delegate type for hooking up synchronized notifications
    public delegate void SynchronizedEventHandler(object sender, FileSystemSynchronizedEventArgs ev);

    public class FileClientSync: FileSyncBase {

        public event AppliedChangeEventHandler AppliedChange;
        public event SkippedChangeEventHandler SkippedChange;
        public event ItemConflictingEventHandler ItemConflicting;
        public event ItemConstraintEventHandler ItemConstraint;
        public event SynchronizedEventHandler Synchronized;

        public SyncDirectionOrder SyncDirection { get; set; }

        public void Synchronize(KnowledgeSyncProvider remoteProvider) {
            Synchronize(SyncDirection, Provider, remoteProvider);
        }

        public void Synchronize(SyncDirectionOrder syncDirection, KnowledgeSyncProvider localProvider, KnowledgeSyncProvider remoteProvider) {
            // Register event handlers
            FileSyncProvider fileSyncProvider = localProvider as FileSyncProvider;
            fileSyncProvider.AppliedChange += new EventHandler<AppliedChangeEventArgs>(OnAppliedChange);
            fileSyncProvider.SkippedChange += new EventHandler<SkippedChangeEventArgs>(OnSkippedChange);

            // Use sync-callbacks for conflicting items
            SyncCallbacks destCallbacks = localProvider.DestinationCallbacks;
            destCallbacks.ItemConflicting += new EventHandler<ItemConflictingEventArgs>(OnItemConflicting);
            destCallbacks.ItemConstraint += new EventHandler<ItemConstraintEventArgs>(OnItemConstraint);

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

        protected virtual void OnAppliedChange(object sender, AppliedChangeEventArgs ev) {
            if (AppliedChange != null)
                AppliedChange(sender, ev);
        }
        
        protected virtual void OnSkippedChange(object sender, SkippedChangeEventArgs ev) {
            if (SkippedChange != null)
                SkippedChange(sender, ev);
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
