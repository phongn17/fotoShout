using FotoShoutUtils.Sync.Files;
using Microsoft.Synchronization.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Files {
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Single)]
    public class FileServerSyncService : FileSyncService, IFileServerSyncService {
        private FileServerSync _serverSync = null;
        
        public override string Ping() {
            return "File Server Sync Service is ready.";
        }

        protected override void Configure(FileSyncInfo syncInfo) {
            Type type = Type.GetType(syncInfo.ServerSyncClass);
            if (type == null)
                throw new FaultException<WebSyncFaultException>(new WebSyncFaultException(string.Format("Server Sync Class {0} is not supported.", syncInfo.ServerSyncClass), null));
            try {
                _serverSync = (FileServerSync)Activator.CreateInstance(type);
                _serverSync.Configure(syncInfo.SyncAction, syncInfo.ReplicaRootPath, syncInfo.Filters, syncInfo.SyncOptions);
                Provider = _serverSync.Provider;
            }
            catch (Exception ex) {
                throw new FaultException<WebSyncFaultException>(new WebSyncFaultException(string.Format("Cannot configure file server sync service for {0} at {1}.", syncInfo.ServerSyncClass, syncInfo.ReplicaRootPath), ex));
            }
        }

        public override void Cleanup() {
            if (_serverSync != null) {
                _serverSync.Dispose();
            }
            base.Cleanup();
        }

        public Guid GetReplicaId() {
            return _serverSync.ReplicaId;
        }
    }
}
