using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FotoShoutUtils.Sync.Db {
    public class DbSynchronizedEventArgs: SynchronizedEventArgs {
        public DbSynchronizedEventArgs(SyncOperationStatistics syncStats)
            : base(syncStats) {
        }
    }
}
