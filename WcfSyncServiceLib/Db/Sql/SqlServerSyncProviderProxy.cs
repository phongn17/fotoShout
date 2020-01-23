using Microsoft.Synchronization.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Db.Sql {
    public class SqlServerSyncProviderProxy: DbSyncProviderProxy {
        private ISqlSyncService _dbProxy = null;

        public SqlServerSyncProviderProxy(string syncAction, string scopeName, string connection, string[] staticTables, string[] dynamicTables, string syncClass, string serviceUri)
            : base(syncAction, scopeName, connection, staticTables, dynamicTables, syncClass, serviceUri) {
        }

        protected override IDbSyncService CreateProxy() {
            ChannelFactory<ISqlSyncService> factory = new ChannelFactory<ISqlSyncService>(ServiceUri);
            ISqlSyncService proxy = factory.CreateChannel();
            _dbProxy = proxy as ISqlSyncService;

            return proxy;
        }

        public DbSyncScopeDescription GetScopeDescription(string syncScopeName) {
            return _dbProxy.GetScopeDescription(syncScopeName);
        }
    }
}
