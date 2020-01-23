using FotoShoutApi.Models;
using FotoShoutApi.Utils;
using FotoShoutData.Models;
using FotoShoutUtils;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace FotoShoutApi.Controllers {
    public class FotoShoutController : ApiController {
        static ILog _logger = LogManager.GetLogger(typeof(FotoShoutController));
        
        public User CurrentUser { get; set; }
        public Guid AuthorizationKey { get; set; }
        public string AccountName { get; set; }

        protected override void Initialize(HttpControllerContext controllerContext) {
            base.Initialize(controllerContext);

            if (Request.RequestUri.AbsolutePath.ToLower().Contains(Constants.FS_API_PREFIX)) {
                try {
                    // get the api key
                    string apiKey = this.GetApiKey(_logger);

                    // validate the key
                    AuthorizationKey = new Guid(apiKey);
                    FotoShoutUtils.Log.LogManager.Info(_logger, "Getting account info...");
                    using (var db = new FotoShoutDbContext()) {
                        CurrentUser = this.GetUser(apiKey, db);
                        if (CurrentUser == null)
                            this.GenerateException(HttpStatusCode.Unauthorized, Errors.ERROR_LOGIN_NOACCOUNT, _logger);

                        Account account = this.GetAccount(CurrentUser.Id, db);
                        if (account == null)
                            this.GenerateException(HttpStatusCode.Unauthorized, Errors.ERROR_AUTHORIZATIONKEY_INVALID, _logger);

                        AccountName = account.AccountName;
                    }
                    FotoShoutUtils.Log.LogManager.Info(_logger, "Successully got account info");

                    Uri uri = controllerContext.Request.RequestUri;
                    AppConfigs.VirtualRoot = uri.Scheme + Uri.SchemeDelimiter + uri.Host + "/";
                }
                catch (HttpResponseException ex) {
                    throw ex;
                }
                catch (Exception ex) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                    this.GenerateException(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }
    }
}