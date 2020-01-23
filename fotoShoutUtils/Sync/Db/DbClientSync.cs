using log4net;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Db {
    public abstract class DbClientSync : DbSync {
        static ILog _logger = LogManager.GetLogger(typeof(DbClientSync));
        public SyncDirectionOrder SyncDirection { get; set; }

        public DbSyncScopeDescription ServerScopeDescription { get; set; }
        
        public virtual void Configure(string syncAction, string scopeName, string connection) {
            SyncScopeName = scopeName;
            Connection = connection;

            if (!string.IsNullOrEmpty(syncAction) && syncAction.Equals(Constants.ASV_SYNCACTION_DEPROVISION, StringComparison.InvariantCultureIgnoreCase)) {
                Log.LogManager.Info(_logger, string.Format("Deprovisioning client sync for the scope {0} ...", scopeName));
                DeProvisionDb();
                Log.LogManager.Info(_logger, "Succesfully deprovisioned client sync");
            }
            else if (!string.IsNullOrEmpty(syncAction) && syncAction.Equals(Constants.ASV_SYNCACTION_DEPROVISIONSTORE, StringComparison.InvariantCultureIgnoreCase)) {
                Log.LogManager.Info(_logger, "Deprovisioning client sync for the entire store");
                DeProvisionStore();
                Log.LogManager.Info(_logger, "Succesfully deprovisioned client sync");
            }
            else {
                Log.LogManager.Info(_logger, string.Format("Provisioning client sync for the scope {0} ...", scopeName));
                ProvisionDb();
                Log.LogManager.Info(_logger, "Succesfully provisioned client sync");
            }
        }

        public void Synchronize(KnowledgeSyncProvider remoteProvider) {
            SynchronizeSyncScope(SyncScopeName, SyncDirection, Provider, remoteProvider);
        }

        public void Synchronize(KnowledgeSyncProvider localProvider, KnowledgeSyncProvider remoteProvider) {
            SynchronizeSyncScope(SyncScopeName, SyncDirection, localProvider, remoteProvider);
        }

        protected abstract void SynchronizeSyncScope(string syncScope, SyncDirectionOrder synDirection, KnowledgeSyncProvider localProvider, KnowledgeSyncProvider remoteProvider);
    }
}
