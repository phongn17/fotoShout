using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Files {
    public abstract class FileSyncEventListener {
        public abstract void AddFileSystemSynchronized(object sender, FileSystemSynchronizedEventArgs ev);
        public abstract void RemoveFileSystemSynchronized(object sender, FileSystemSynchronizedEventArgs ev);
    }
}
