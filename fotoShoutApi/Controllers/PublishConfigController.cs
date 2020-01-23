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

namespace FotoShoutApi.Controllers
{
    public class PublishConfigController : FotoShoutController
    {
        static ILog _logger = LogManager.GetLogger(typeof(PublishConfigController));
        
        private FotoShoutDbContext db = new FotoShoutDbContext();

        // GET api/PublishConfig
        public PublishAccount GetPublishAccount() {
            PublishAccount publishAccount = null;
            try {
                publishAccount = db.PublishAccounts.Where(p => p.Account.Id == CurrentUser.Account.Id).SingleOrDefault();
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.InternalServerError, ex.Message);
            }

            if (publishAccount == null) {
                this.GenerateException(HttpStatusCode.NotFound, "Publish Account has not been configured yet.");
            }

            return publishAccount;
        }

        // PUT api/PublishConfig/5
        public HttpResponseMessage PutPublishAccount(int id, PublishAccount publishAccount) {
            if (!ModelState.IsValid) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ModelState.Values.SelectMany(v => v.Errors));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != publishAccount.Id) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format(Errors.ERROR_PUBLISHACCOUNT_UPDATE_IDSNOTIDENTICAL, id, publishAccount.Id));
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Errors.ERROR_PUBLISHACCOUNT_UPDATE_IDSNOTIDENTICAL, id, publishAccount.Id));
            }

            db.Entry(publishAccount).State = EntityState.Modified;

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

        // POST api/PublishConfig
        public HttpResponseMessage PostPublishAccount(PublishAccount publishAccount)
        {
            if (ModelState.IsValid) {
                publishAccount.Account = db.Accounts.Find(CurrentUser.Account.Id);
                if (publishAccount.Account == null) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, Errors.ERROR_NOACCOUNT);
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, Errors.ERROR_NOACCOUNT);
                }
                db.PublishAccounts.Add(publishAccount);
                try {
                    db.SaveChanges();
                }
                catch (Exception ex) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message));
                }

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, publishAccount);
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