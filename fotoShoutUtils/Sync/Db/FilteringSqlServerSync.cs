using FotoShoutUtils.Service;
using FotoShoutUtils.Sync.Db;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Db {
    public class FilteringSqlServerSync: SqlServerSync {
        private Filtering _filtering = new Filtering();
        
        protected override void ApplyStaticFilters(SqlSyncScopeProvisioning serverProvision, string syncScope) {
            _filtering.ApplyStaticFilters(serverProvision, syncScope);
        }
    }
}
