using FotoShoutUtils;
using FotoShoutUtils.Sync.Db;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Db.Sql {
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Single)]
    public class SqlServerSyncService: DbSyncService, ISqlSyncService {
        private SqlServerSync _serverSync = null;

        public override string Ping() {
            return "Sql Sync Service is ready.";
        }

        protected override void Configure(DbSyncInfo syncInfo) {
            Type type = Type.GetType(syncInfo.ServerSyncClass);
            if (type == null)
                throw new FaultException<WebSyncFaultException>(new WebSyncFaultException(string.Format("Server Sync Class {0} is not supported.", syncInfo.ServerSyncClass), null));
            _serverSync = (SqlServerSync)Activator.CreateInstance(type);
            _serverSync.Configure(syncInfo.SyncAction, syncInfo.SyncScopeName, syncInfo.Connection, syncInfo.StaticSyncTables, syncInfo.DynamicSyncTables);
            Provider = _serverSync.Provider;
        }
        
        public DbSyncScopeDescription GetScopeDescription(string syncScopeName) {
            return _serverSync.GetScopeDescription(syncScopeName);
        }
    }
}
