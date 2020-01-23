using Microsoft.Synchronization.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Db.SqlCe {
    public class CeSyncProviderProxy : DbSyncProviderProxy {
        private const int READERQUOTAS_MAXARRAY = 100000;
        private const int MESSAGESIZE_MAX = 10485760;

        private IDbClientSyncService _ceProxy = null;
        private DbSyncScopeDescription _scopeDescription = null;

        public CeSyncProviderProxy(string syncAction, string scopeName, string connection, DbSyncScopeDescription scopeDescription, string serviceUri)
            : base(syncAction, scopeName, connection, null, null, null, serviceUri) {
            _scopeDescription = scopeDescription;
        }
        
        public void CreateScopeDescription(DbSyncScopeDescription scopeDescription) {
            _ceProxy.CreateScopeDescription(scopeDescription);
        }

        public bool NeedsScope() {
            return _ceProxy.NeedsScope();
        }

        public void GenerateSnapshot(String destinationFileName) {
            _ceProxy.GenerateSnapshot(destinationFileName);
        }
        
        protected override IDbSyncService CreateProxy() {
            WSHttpBinding binding = new WSHttpBinding();
            binding.ReaderQuotas.MaxArrayLength = CeSyncProviderProxy.READERQUOTAS_MAXARRAY;
            binding.MaxReceivedMessageSize = CeSyncProviderProxy.MESSAGESIZE_MAX;
            ChannelFactory<IDbClientSyncService> factory = new ChannelFactory<IDbClientSyncService>(binding, new EndpointAddress(ServiceUri));
            IDbClientSyncService proxy = factory.CreateChannel();
            _ceProxy = proxy as IDbClientSyncService;

            return proxy;
        }
    }
}
