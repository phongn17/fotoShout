using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Db {
    public abstract class SqlSync: DbSync {
        public override System.Data.Common.DbConnection DbConnection {
            get {
                if (string.IsNullOrEmpty(Connection))
                    throw new ArgumentNullException("connection");

                return new SqlConnection(Connection);
            }
        }

        public abstract void Configure(string syncAction, string syncScopeName, string connection, string[] staticSyncTables = null, string[] dynamicSyncTables = null);

        public SqlSyncScopeProvisioning CreateScopeProvision(DbSyncScopeDescription scopeDescription = null) {
            return ((scopeDescription == null) ? new SqlSyncScopeProvisioning(DbConnection as SqlConnection) : new SqlSyncScopeProvisioning(DbConnection as SqlConnection, scopeDescription));
        }

        public SqlSyncScopeProvisioning CreateScopeProvisionByType(SqlSyncScopeProvisioningType provisioningType, DbSyncScopeDescription scopeDescription = null) {
            return ((scopeDescription == null) ? new SqlSyncScopeProvisioning(DbConnection as SqlConnection, provisioningType) : new SqlSyncScopeProvisioning(DbConnection as SqlConnection, scopeDescription, provisioningType));
        }

        public SqlSyncScopeDeprovisioning CreateScopeDeProvision() {
            return new SqlSyncScopeDeprovisioning(DbConnection as SqlConnection);
        }

        public DbSyncScopeDescription GetScopeDescription(string syncScopeName) {
            SqlSyncScopeProvisioning serverProvision = CreateScopeProvision();
            return (serverProvision.ScopeExists(syncScopeName) ? SqlSyncDescriptionBuilder.GetDescriptionForScope(syncScopeName, (SqlConnection)DbConnection) : null);
        }
    }
}
