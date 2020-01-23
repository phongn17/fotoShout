using Microsoft.Synchronization.Data.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Db {
    public class Filtering {
        private List<string> _filters = null;

        public virtual void ApplyStaticFilters(SqlSyncScopeProvisioning provision, string syncScope) {
            GetFilters(syncScope);
            if (_filters != null && _filters.Count > 0) {
                foreach (SqlSyncTableProvisioning syncTable in provision.Tables) {
                    if (syncTable.UnquotedLocalName.Equals(Constants.TABLE_EMAILTEMPLATES, StringComparison.InvariantCultureIgnoreCase) ||
                        syncTable.UnquotedLocalName.Equals(Constants.TABLE_EVENTS, StringComparison.InvariantCultureIgnoreCase)) {
                        syncTable.AddFilterColumn("User_Id");
                        syncTable.FilterClause = "[side].[User_Id] = " + _filters[0];
                    }
                    else if (syncTable.UnquotedLocalName.Equals(Constants.TABLE_GUESTS, StringComparison.InvariantCultureIgnoreCase) ||
                        syncTable.UnquotedLocalName.Equals(Constants.TABLE_GUESTPHOTOES, StringComparison.InvariantCultureIgnoreCase)) {
                        syncTable.AddFilterColumn("Event_EventId");
                        syncTable.FilterClause = "[side].[Event_EventId] = " + _filters[0];
                    }
                    else if (syncTable.UnquotedLocalName.Equals(Constants.TABLE_PHOTOS, StringComparison.InvariantCultureIgnoreCase)) {
                        syncTable.AddFilterColumn("Event_EventId");
                        syncTable.FilterClause = "[side].[Event_EventId] = " + _filters[0];
                    }
                    else if (syncTable.UnquotedLocalName.Equals("Users", StringComparison.InvariantCultureIgnoreCase)) {
                        syncTable.AddFilterColumn("Id");
                        syncTable.FilterClause = "[side].[Id] = " + _filters[0];
                    }
                    else if (syncTable.UnquotedLocalName.Equals("UserAuthorizations", StringComparison.InvariantCultureIgnoreCase)) {
                        syncTable.AddFilterColumn("Id");
                        syncTable.FilterClause = "[side].[Id] = " + _filters[1];
                    }
                    else if (syncTable.UnquotedLocalName.Equals("Accounts", StringComparison.InvariantCultureIgnoreCase)) {
                        syncTable.AddFilterColumn("Id");
                        syncTable.FilterClause = "[side].[Id] = " + _filters[2];
                    }
                }
            }
        }

        private void GetFilters(string syncScope) {
            int idx = syncScope.IndexOf("_");
            if (idx != -1) {
                string filtersStr = syncScope.Substring(idx + 1);
                string[] filters = filtersStr.Split(new char[] { '_' });
                if (filters.Length >= 1) {
                    if (_filters == null)
                        _filters = new List<string>();
                    else
                        _filters.Clear();
                    for (int pos = 0; pos < filters.Length; pos++) {
                        _filters.Add(filters[pos]);
                    }
                }
            }
        }
    }
}
