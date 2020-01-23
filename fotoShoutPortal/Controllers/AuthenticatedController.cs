using FotoShoutData.Models;
using FotoShoutData.Models.Authenticate;
using FotoShoutPortal.Models;
using FotoShoutUtils.Service;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace FotoShoutPortal.Controllers {
    public class AuthenticatedController : Controller {
        private ILog _logger = LogManager.GetLogger(typeof(AuthenticatedController));
        
        protected FsApiWebService fsWebService = new FsApiWebService(AppConfigs.ApiBaseAddress, AppConfigs.ApiPrefix, FotoShoutUtils.Constants.MEDIATYPE_APPLICATION_JSON);
        protected PublishApiWebService publishWebService = null;
        
        protected override void Initialize(System.Web.Routing.RequestContext requestContext) {
            base.Initialize(requestContext);

            if (Request.IsAuthenticated) {
                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null) {
                    InitAuthentication(authCookie);
                }
            }
            if (string.IsNullOrEmpty(fsWebService.Authorization)) {
                FormsAuthentication.SignOut();
                FormsAuthentication.RedirectToLoginPage();
                Response.End();
            }
        }

        public PublishApiWebService GeneratePublishService() {
            try {
                PublishAccount publishAccount = fsWebService.GetPublishConfiguration();
                if (publishAccount != null && !string.IsNullOrEmpty(publishAccount.Url)) {
                    return GeneratePublishService(publishAccount);
                }
            }
            catch (Exception ex) {
                if (!ex.Message.Equals("Not Found", StringComparison.InvariantCultureIgnoreCase))
                    FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
            }

            return null;
        }

        public PublishApiWebService GeneratePublishService(PublishAccount publishAccount) {
            string url = publishAccount.Url.EndsWith("/") ? publishAccount.Url.Substring(0, publishAccount.Url.Length - 1) : publishAccount.Url;
            if (!url.StartsWith("http://") && url.StartsWith("https://"))
                throw new WebServiceException("The provided url must be started with 'http://' or 'https://'");

            int lastIdx = url.LastIndexOf("/");
            if (lastIdx != -1) {
                string baseAddress = url.Substring(0, lastIdx + 1);
                if (baseAddress.Equals("http://") || baseAddress.Equals("https://"))
                    throw new WebServiceException(string.Format("The \"{0}\" url is incorrect.", publishAccount.Url));

                string prefix = url.Substring(lastIdx + 1);
                PublishApiWebService temp = new PublishApiWebService(baseAddress, prefix, FotoShoutUtils.Constants.MEDIATYPE_APPLICATION_JSON);
                string ret = temp.Authenticate(publishAccount.ApiKey.ToString(), new LoginModel {
                    UserName = publishAccount.Email,
                    Password = publishAccount.Password
                });

                return temp;
            }
            else {
                throw new WebServiceException(string.Format("The \"{0}\" url is incorrect.", publishAccount.Url));
            }
        }

        protected void InitAuthentication(HttpCookie authCookie) {
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
            string fsAuthorization = AuthenticationWebService.ExtractAuthorizationString(ticket.UserData);
            if (string.IsNullOrEmpty(fsAuthorization))
                return;

            fsWebService.Authorization = fsAuthorization;
            publishWebService = GeneratePublishService();
        }
    }
}
