using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FotoShoutUtils.Sync {
    public class SynchronizedEventArgs {
        private readonly SyncOperationStatistics _syncStats;

        public SynchronizedEventArgs(SyncOperationStatistics syncStats) {
            _syncStats = syncStats;
        }

        public SyncOperationStatistics SyncStats {
            get {
                return _syncStats;
            }
        }
    }
}
