using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Db {
    public abstract class DbSyncEventListener {
        public abstract void AddDbSynchronized(object sender, DbSynchronizedEventArgs ev);
        public abstract void RemoveDbSynchronized(object sender, DbSynchronizedEventArgs ev);
    }
}
