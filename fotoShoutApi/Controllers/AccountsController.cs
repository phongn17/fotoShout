using FotoShoutApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using FotoShoutApi.Utils;
using FotoShoutData.Models;
using FotoShoutUtils.Utils;
using log4net;

namespace FotoShoutApi.Controllers {
    public class AccountsController : ApiController {
        static ILog _logger = LogManager.GetLogger(typeof(AccountsController));
        
        private FotoShoutDbContext db = new FotoShoutDbContext();

        public IEnumerable<Account> GetAccounts() {
            throw new NotImplementedException();
        }

        public Account GetAccountByName(string name) {
            Account account = null;
            try {
                account = db.Accounts.Where(a => a.AccountName.Equals(name, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.InternalServerError, ex.Message);
            }
            if (account == null)
                this.GenerateException(HttpStatusCode.NotFound, string.Format("The \"{0}\" account not found.", name));

            return account;
        }

        public Account GetAccountById(int id) {
            Account account = null;
            try {
                account = db.Accounts.Find(id);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.InternalServerError, ex.Message);
            }
            if (account == null)
                this.GenerateException(HttpStatusCode.NotFound, string.Format("An account with the {0} id not found.", id));

            return account;
        }

        protected override void Dispose(bool disposing) {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}
