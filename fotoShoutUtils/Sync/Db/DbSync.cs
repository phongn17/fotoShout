using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Db {
    // A delegate type for hooking up item conflicting notifications
    public delegate void ItemConflictingEventHandler(object sender, ItemConflictingEventArgs ev);
    // A delegate type for hooking up notifications when an item constraint is violated
    public delegate void ItemConstraintEventHandler(object sender, ItemConstraintEventArgs ev);
    // A delegate type for hooking up change-failed notifications
    public delegate void ApplyChangeFailedEventHandler(object sender, DbApplyChangeFailedEventArgs ev);
    // A delegate type for hooking up changes-applied notifications
    public delegate void ChangesAppliedEventHandler(object sender, DbChangesAppliedEventArgs ev);
    // A delegate type for hooking up synchronized notifications
    public delegate void SynchronizedEventHandler(object sender, DbSynchronizedEventArgs ev);

    public abstract class DbSync {
        // An event that client can use to be notified whenever there is an item conflicting
        public event ItemConflictingEventHandler ItemConflicting;
        // An event that client can use to be notified whenever an item constraint is violated
        public event ItemConstraintEventHandler ItemConstraint;
        // An event that client can use to be notified whenever a change is failed to apply to the client
        public event ApplyChangeFailedEventHandler ApplyChangeFailed;
        // An event that client can use to be notified whenever changes applied to the client
        public event ChangesAppliedEventHandler ChangesApplied;
        // An event that client can use to be notified whenever a synchronization takes place
        public event SynchronizedEventHandler Synchronized;

        public string Connection { get; set; }
        public string SyncScopeName { get; set; }
        public RelationalSyncProvider Provider { get; set; }

        public abstract DbConnection DbConnection { get; }

        public abstract void ProvisionDb();
        public abstract void DeProvisionDb();
        public abstract void DeProvisionStore();

        protected virtual void OnApplyChangeFailed(object sender, DbApplyChangeFailedEventArgs ev) {
            if (ApplyChangeFailed != null)
                ApplyChangeFailed(sender, ev);
        }

        protected virtual void OnChangesApplied(object sender, DbChangesAppliedEventArgs ev) {
            if (ChangesApplied != null)
                ChangesApplied(sender, ev);
        }

        protected virtual void OnSynchronized(object sender, DbSynchronizedEventArgs ev) {
            if (Synchronized != null)
                Synchronized(sender, ev);
        }
        
        protected virtual void OnItemConfliting(object sender, ItemConflictingEventArgs ev) {
            if (ItemConflicting != null)
                ItemConflicting(sender, ev);
        }

        protected virtual void OnItemConstraint(object sender, ItemConstraintEventArgs ev) {
            if (ItemConstraint != null)
                ItemConstraint(sender, ev);
        }
    }
}
