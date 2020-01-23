using FotoShoutUtils;
using FotoShoutUtils.Sync.Db;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using Microsoft.Synchronization.Data.SqlServerCe;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Db.Sql {
    public class SqlClientSyncService: DbSyncService, IDbClientSyncService {
        private SqlClientSync _clientSync = new SqlClientSync();

        public override string Ping() {
            return "Sql Client Sync Service is ready.";
        }

        public void Synchronize(KnowledgeSyncProvider remoteProvider) {
            _clientSync.Synchronize(remoteProvider);
        }

        protected override void Configure(DbSyncInfo syncInfo) {
            _clientSync.Configure(syncInfo.SyncAction, syncInfo.SyncScopeName, syncInfo.Connection);
            Provider = _clientSync.Provider;
        }
        
        public void CreateScopeDescription(DbSyncScopeDescription scopeDescription) {
            SqlSyncScopeProvisioning scopeProvision = _clientSync.CreateScopeProvision(scopeDescription);
            scopeProvision.Apply();
        }

        public void GenerateSnapshot(string destFileName) {
            throw new FaultException<WebSyncFaultException>(new WebSyncFaultException("Not implemented yet", null));
        }

        public bool NeedsScope() {
            SqlSyncScopeProvisioning scopeProvision = _clientSync.CreateScopeProvision();
            return (!scopeProvision.ScopeExists(_clientSync.SyncScopeName));
        }

        public void SetServerScopeDescription(DbSyncScopeDescription scopeDescription) {
            _clientSync.ServerScopeDescription = scopeDescription;
        }

        public void SetSyncDirection(SyncDirectionOrder syncDirection) {
            _clientSync.SyncDirection = syncDirection;
        }
    }
}
