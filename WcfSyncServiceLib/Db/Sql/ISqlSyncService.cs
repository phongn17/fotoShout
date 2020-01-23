using Microsoft.Synchronization.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Db.Sql {
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface ISqlSyncService: IDbSyncService {
        [OperationContract]
        [FaultContract(typeof(WebSyncFaultException))]
        DbSyncScopeDescription GetScopeDescription(string syncScopeName);
    }
}
