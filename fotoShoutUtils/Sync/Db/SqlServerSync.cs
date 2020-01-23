using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Db {
    public class SqlServerSync: SqlSync {
        public SqlServerSync() {
            StaticSyncTables = new HashSet<string>();
            DynamicSyncTables = new HashSet<string>();
        }

        public ICollection<string> StaticSyncTables { get; private set; }
        public ICollection<string> DynamicSyncTables { get; private set; }

        public override void Configure(string syncAction, string syncScopeName, string connection, string[] staticSyncTables = null, string[] dynamicSyncTables = null) {
            SyncScopeName = syncScopeName;
            Connection = connection;

            if (!string.IsNullOrEmpty(syncAction) && syncAction.Equals(Constants.ASV_SYNCACTION_DEPROVISION, StringComparison.InvariantCultureIgnoreCase)) {
                DeProvisionDb();
            }
            else if (!string.IsNullOrEmpty(syncAction) && syncAction.Equals(Constants.ASV_SYNCACTION_DEPROVISIONSTORE, StringComparison.InvariantCultureIgnoreCase)) {
                DeProvisionStore();
            }
            else {
                // sync tables
                if (staticSyncTables != null) {
                    foreach (string syncTable in staticSyncTables)
                        AddSyncTable(syncTable, true);
                }
                if (dynamicSyncTables != null) {
                    foreach (string syncTable in dynamicSyncTables)
                        AddSyncTable(syncTable, false);
                }

                ProvisionDb();

                Provider = new SqlSyncProvider(syncScopeName, DbConnection as SqlConnection);
            }
        }

        public void AddSyncTable(string tableName, bool isStatic) {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("server sync table");
            
            if (isStatic)
                StaticSyncTables.Add(tableName);
            else
                DynamicSyncTables.Add(tableName);
        }

        public override void ProvisionDb() {
            using (SqlConnection serverConnect = (SqlConnection)DbConnection) {
                // Create a server scope provisioning object
                SqlSyncScopeProvisioning serverProvision;
                if (StaticSyncTables.Count > 0) {
                    serverProvision = CreateScopeProvision();
                    ProvisionSyncScope(serverProvision, SyncScopeName, StaticSyncTables, serverConnect, SqlSyncScopeProvisioningType.Scope);
                }
                if (DynamicSyncTables.Count > 0) {
                    serverProvision = CreateScopeProvisionByType(SqlSyncScopeProvisioningType.Template);
                    ProvisionSyncScope(serverProvision, SyncScopeName + "_Template", DynamicSyncTables, serverConnect, SqlSyncScopeProvisioningType.Template);
                }
            }
        }

        public override void DeProvisionDb() {
            SqlSyncScopeProvisioning serverProvision = CreateScopeProvision();
            SqlSyncScopeDeprovisioning serverDeprovision = null;
            if (serverProvision.ScopeExists(SyncScopeName)) {
                serverDeprovision = CreateScopeDeProvision();
                serverDeprovision.DeprovisionScope(SyncScopeName);
            }
            serverProvision = CreateScopeProvisionByType(SqlSyncScopeProvisioningType.Template);
            if (serverProvision.ScopeExists(SyncScopeName + "_Template")) {
                if (serverDeprovision == null)
                    serverDeprovision = CreateScopeDeProvision();
                serverDeprovision.DeprovisionScope(SyncScopeName);
            }
        }

        public override void DeProvisionStore() {
            SqlSyncScopeDeprovisioning serverDeprovision = CreateScopeDeProvision();
            serverDeprovision.DeprovisionStore();
        }

        protected virtual void ProvisionSyncScope(SqlSyncScopeProvisioning serverProvision, string syncScope, ICollection<string> syncTables, SqlConnection serverConnect, SqlSyncScopeProvisioningType provisionType) {
            // Create a sync scope if it is not existed yet
            if (!string.IsNullOrEmpty(syncScope) && syncTables != null && syncTables.Any()) {
                // Check if the sync scope or template exists
                if (provisionType == SqlSyncScopeProvisioningType.Scope && serverProvision.ScopeExists(syncScope))
                    return;
                if (provisionType == SqlSyncScopeProvisioningType.Template && serverProvision.TemplateExists(syncScope))
                    return;

                // Define a new sync scope
                DbSyncScopeDescription scopeDesc = new DbSyncScopeDescription(syncScope);

                // Generate and add table descriptions to the sync scope
                foreach (string tblName in syncTables) {
                    // Get the description of a specific table
                    DbSyncTableDescription tblDesc = SqlSyncDescriptionBuilder.GetDescriptionForTable(tblName, serverConnect);
                    // add the table description to the sync scope
                    scopeDesc.Tables.Add(tblDesc);
                }

                // Set the scope description from which the database should be provisioned
                serverProvision.PopulateFromScopeDescription(scopeDesc);
                if (provisionType == SqlSyncScopeProvisioningType.Template) {
                    serverProvision.ObjectSchema = "Sync";
                    // apply dynamic filters
                    ApplyDynamicFilters(serverProvision, syncScope);
                }
                else {
                    // apply static filters
                    ApplyStaticFilters(serverProvision, syncScope);
                }

                // Indicate that the base table already exists and does not need to be created
                serverProvision.SetCreateTableDefault(DbSyncCreationOption.Skip);
                serverProvision.SetCreateProceduresForAdditionalScopeDefault(DbSyncCreationOption.Create);

                // start the provisioning process
                serverProvision.Apply();
            }
        }

        protected virtual void ApplyStaticFilters(SqlSyncScopeProvisioning serverProvision, string syncScope) {
        }
        
        protected virtual void ApplyDynamicFilters(SqlSyncScopeProvisioning serverProvision, string syncScope) {
        }
    }
}
