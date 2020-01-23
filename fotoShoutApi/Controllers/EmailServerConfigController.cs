using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FotoShoutData.Models;
using FotoShoutApi.Models;
using log4net;

namespace FotoShoutApi.Controllers {
    public class EmailServerConfigController : FotoShoutController {
        static ILog _logger = LogManager.GetLogger(typeof(EmailServerConfigController));
        private FotoShoutDbContext db = new FotoShoutDbContext();

        // GET api/EmailServerConfig
        public EmailServerAccount GetEmailServerAccount() {
            EmailServerAccount emailServerAccount = null;
            try {
                emailServerAccount = db.EmailServerAccounts.Where(e => e.User.Id == CurrentUser.Id).SingleOrDefault();
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.InternalServerError, ex.Message);
            }

            if (emailServerAccount == null) {
                this.GenerateException(HttpStatusCode.NotFound, "Email Server Account has not been configured yet.");
            }

            return emailServerAccount;
        }

        // PUT api/EmailServerConfig/5
        public HttpResponseMessage PutEmailServerAccount(int id, EmailServerAccount emailServerAccount) {
            if (!ModelState.IsValid) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ModelState.Values.SelectMany(v => v.Errors));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != emailServerAccount.EmailServerAccountId) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format(Errors.ERROR_EMAILSERVERCONFIG_UPDATE_IDSNOTIDENTICAL, id, emailServerAccount.EmailServerAccountId));
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(emailServerAccount).State = EntityState.Modified;

            try {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message));
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/EmailServerConfig
        public HttpResponseMessage PostEmailServerAccount(EmailServerAccount emailServerAccount) {
            if (ModelState.IsValid) {
                emailServerAccount.User = db.Users.Find(CurrentUser.Id);
                db.EmailServerAccounts.Add(emailServerAccount);
                try {
                    db.SaveChanges();
                }
                catch (Exception ex) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message));
                }

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, emailServerAccount);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", null));
                return response;
            }
            else {
                FotoShoutUtils.Log.LogManager.Error(_logger, ModelState.Values.SelectMany(e => e.Errors));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}