using FotoShoutApi.Models;
using FotoShoutData.Models;
using FotoShoutUtils;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;

namespace FotoShoutApi.Controllers {
    public static class ApiControllerExtensions {
        public static HttpResponseException GenerateException(this ApiController controller, HttpStatusCode statusCode, string msg, ILog logger = null) {
            if (logger != null)
                FotoShoutUtils.Log.LogManager.Error(logger, msg);

            msg = Regex.Replace(msg, @"\r*\n*", "");
            var resp = new HttpResponseMessage(statusCode) {
                Content = new StringContent(msg),
                ReasonPhrase = msg
            };
            throw new HttpResponseException(resp);
        }

        public static string GetApiKey(this ApiController controller, ILog logger) {
            // get the api key
            string apiKey = "";
            FotoShoutUtils.Log.LogManager.Info(logger, "Getting authorization key...");
            if (controller.ControllerContext.Request.Headers.Contains(Constants.FS_AUTHORIZATION_KEY))
                apiKey = controller.ControllerContext.Request.Headers.GetValues(Constants.FS_AUTHORIZATION_KEY).First();

            // use the authorization header if it is there
            if (controller.ControllerContext.Request.Headers.Authorization != null)
                apiKey = controller.ControllerContext.Request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(apiKey))
                controller.GenerateException(HttpStatusCode.Unauthorized, Errors.ERROR_AUTHORIZATIONKEY_EMPTY, logger);

            FotoShoutUtils.Log.LogManager.Info(logger, "Successfully got authorization key");
            return apiKey;
        }

        public static User GetUser(this ApiController controller, string apiKey, FotoShoutDbContext db) {
            return (from u in db.Users join ua in db.UserAuthorizations on u.Authorization.Id equals ua.Id where ua.AuthorizationKey == new Guid(apiKey) select u).SingleOrDefault();
        }

        public static Account GetAccount(this ApiController controller, int userId, FotoShoutDbContext db) {
            return (from u in db.Users join a in db.Accounts on u.Account.Id equals a.Id where u.Id == userId select a).SingleOrDefault();
        }
    }
}