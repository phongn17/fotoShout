using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Files {
    public class FileSyncBase: IDisposable {
        public void Configure(string syncAction, string replicaRootPath, string[] filters, FileSyncOptions syncOptions) {
            ReplicaRootPath = replicaRootPath;
            SetScopeFilter(filters);
            SyncOptions = syncOptions;

            DetectChanges();
        }
        
        public string ReplicaRootPath { get; set; }

        public FileSyncScopeFilter ScopeFilter { get; set; }
        public FileSyncOptions SyncOptions { get; set; }

        public Guid ReplicaId { get; set; }

        public FileSyncProvider Provider { get; set; }
        
        public void SetScopeFilter(string[] filters) {
            FileSyncScopeFilter scopeFilter = new FileSyncScopeFilter();
            foreach (string filter in filters)
                scopeFilter.FileNameIncludes.Add(filter);
            ScopeFilter = scopeFilter;
        }

        public void DetectChanges() {
            ReplicaId = DetectChanges(ReplicaRootPath, ScopeFilter, SyncOptions);
        }

        protected virtual FileSyncProvider GenerateProvider() {
            SyncId syncId = new SyncId(Guid.NewGuid());
            return new FileSyncProvider(syncId.GetGuidId(), ReplicaRootPath, ScopeFilter, SyncOptions);
        }
        
        private Guid DetectChanges(string replicaRootPath, FileSyncScopeFilter filter, FileSyncOptions options) {
            if (Provider == null)
                Provider = GenerateProvider();
            
            Provider.DetectChanges();

            return Provider.ReplicaId;
        }

        public void Dispose() {
            if (Provider != null) {
                Provider.Dispose();
                Provider = null;
            }
        }
    }
}
