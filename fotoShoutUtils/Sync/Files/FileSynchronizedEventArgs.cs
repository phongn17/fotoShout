using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FotoShoutUtils.Sync.Files {
    public class FileSystemSynchronizedEventArgs: SynchronizedEventArgs {
        public FileSystemSynchronizedEventArgs(SyncOperationStatistics syncStats)
            : base(syncStats) {
        }
    }
}
