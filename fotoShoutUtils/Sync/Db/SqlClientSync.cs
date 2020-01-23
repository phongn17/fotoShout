using log4net;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Db {
    public class SqlClientSync: DbClientSync {
        static ILog _logger = LogManager.GetLogger(typeof(SqlClientSync));
        public override System.Data.Common.DbConnection DbConnection {
            get {
                if (string.IsNullOrEmpty(Connection))
                    throw new ArgumentNullException("client connection");

                return new SqlConnection(Connection);
            }
        }

        public override void Configure(string syncAction, string scopeName, string connection) {
            base.Configure(syncAction, scopeName, connection);
            if (string.IsNullOrEmpty(syncAction) || syncAction.Equals(Constants.ASV_SYNCACTION_PROVISION, StringComparison.InvariantCultureIgnoreCase))
                Provider = new SqlSyncProvider(SyncScopeName, DbConnection as SqlConnection);
        }

        public SqlSyncScopeProvisioning CreateScopeProvision(DbSyncScopeDescription scopeDescription = null) {
            return ((scopeDescription == null) ? new SqlSyncScopeProvisioning(DbConnection as SqlConnection) : new SqlSyncScopeProvisioning(DbConnection as SqlConnection, scopeDescription));
        }

        public SqlSyncScopeDeprovisioning CreateScopeDeProvision() {
            return new SqlSyncScopeDeprovisioning(DbConnection as SqlConnection);
        }

        public override void ProvisionDb() {
            if (ServerScopeDescription == null)
                throw new ArgumentNullException("server scope description");

            SqlSyncScopeProvisioning clientProvision = CreateScopeProvision();
            ProvisionSyncScope(clientProvision, ServerScopeDescription, SqlSyncScopeProvisioningType.Scope);
        }

        public override void DeProvisionDb() {
            SqlSyncScopeProvisioning clientProvision = CreateScopeProvision();
            if (clientProvision.ScopeExists(SyncScopeName)) {
                SqlSyncScopeDeprovisioning clientDeprovision = CreateScopeDeProvision();
                clientDeprovision.DeprovisionScope(SyncScopeName);
            }
        }

        public override void DeProvisionStore() {
            SqlSyncScopeDeprovisioning clientDeprovision = CreateScopeDeProvision();
            clientDeprovision.DeprovisionStore();
        }

        protected virtual void ProvisionSyncScope(SqlSyncScopeProvisioning clientProvision, DbSyncScopeDescription serverScopeDescription, SqlSyncScopeProvisioningType provisionType) {
            if (serverScopeDescription != null) {
                // Create provisioning object
                if (!clientProvision.ScopeExists(serverScopeDescription.ScopeName)) {
                    clientProvision.PopulateFromScopeDescription(serverScopeDescription);
                    if (provisionType == SqlSyncScopeProvisioningType.Scope)
                        ApplyStaticFilters(clientProvision, serverScopeDescription.ScopeName);
                    clientProvision.SetCreateProceduresForAdditionalScopeDefault(DbSyncCreationOption.Create);
                    // Start the provisioning process
                    clientProvision.Apply();
                }
            }
        }

        protected override void SynchronizeSyncScope(string syncScope, SyncDirectionOrder synDirection, KnowledgeSyncProvider localProvider, KnowledgeSyncProvider remoteProvider) {
            // Create the sync orchestrator
            SyncOrchestrator orchestrator = new SyncOrchestrator();
            // Set local provider of orchestrator to a sync provider associated with the sync scope in the client database
            orchestrator.LocalProvider = localProvider;
            // Set remote provider of orchestrator to a sync provider associated with the sync scope in the server database
            orchestrator.RemoteProvider = remoteProvider;
            // Set the direction of sync session
            orchestrator.Direction = synDirection;

            // Use sync-callbacks for conflicting items
            SyncCallbacks destCallbacks = ((KnowledgeSyncProvider)orchestrator.RemoteProvider).DestinationCallbacks;
            destCallbacks.ItemConflicting += new EventHandler<ItemConflictingEventArgs>(OnItemConfliting);
            destCallbacks.ItemConstraint += new EventHandler<ItemConstraintEventArgs>(OnItemConstraint);

            // Subcribe for errors that occur when applying changes to the client
            ((SqlSyncProvider)orchestrator.LocalProvider).ApplyChangeFailed += new EventHandler<DbApplyChangeFailedEventArgs>(OnApplyChangeFailed);
            ((SqlSyncProvider)orchestrator.LocalProvider).ChangesApplied += new EventHandler<DbChangesAppliedEventArgs>(OnChangesApplied);

            // Execute the synchronize process
            SyncOperationStatistics syncStats = orchestrator.Synchronize();

            // Notify a synchronization took place
            DbSynchronizedEventArgs ev = new DbSynchronizedEventArgs(syncStats);
            OnSynchronized(orchestrator, ev);
                    
            destCallbacks.ItemConflicting -= new EventHandler<ItemConflictingEventArgs>(OnItemConfliting);
            destCallbacks.ItemConstraint -= new EventHandler<ItemConstraintEventArgs>(OnItemConstraint);
            ((SqlSyncProvider)orchestrator.LocalProvider).ApplyChangeFailed -= new EventHandler<DbApplyChangeFailedEventArgs>(OnApplyChangeFailed);
        }
        
        protected virtual void ApplyStaticFilters(SqlSyncScopeProvisioning provision, string syncScope) {
        }

        protected virtual void ApplyDynamicFilters(SqlSyncScopeProvisioning provision, string syncScope) {
        }
    }
}
