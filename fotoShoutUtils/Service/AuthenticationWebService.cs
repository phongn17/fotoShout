using FotoShoutData.Models;
using FotoShoutData.Models.Authenticate;
using FotoShoutUtils.Service;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutUtils.Service {
    public abstract class AuthenticationWebService: WebClientService {
        public static string ExtractAuthorizationString(string auth) {
            return (!string.IsNullOrEmpty(auth) ? auth.Trim(new char[] { '\"' }) : "");
        }

        private ILog _logger = null;
        
        public AuthenticationWebService(string baseAddress, string prefix, string mediaType, ILog logger) 
            : base(baseAddress, prefix, mediaType) {
            _logger = logger;
        }

        public string AuthorizationHeader { get; set; }

        public string Authorization {
            get {
                return ((this.Headers != null) ? this.Headers[AuthorizationHeader] : "");
            }
            set {
                Dictionary<string, string> headers = this.Headers ?? null;
                if (headers == null) {
                    headers = new Dictionary<string, string>();
                    this.Headers = headers;
                }
                if (!headers.ContainsKey(AuthorizationHeader))
                    headers.Add(AuthorizationHeader, value);
                else
                    headers[AuthorizationHeader] = value;
            }
        }

        public virtual string Authenticate(string apiKey, LoginModel model) {
            return Authenticate(GenerateCredentials(apiKey, model));
        }

        public virtual string Authenticate(object credentials) {
            // Need to switch back
            string ret = UploadString("Authorization", JsonConvert.SerializeObject(credentials));
            //string ret = "F6D45E2F-9B6A-4EDA-81FF-046AC55B2E02";
            ret = AuthenticationWebService.ExtractAuthorizationString(ret);
            if (!string.IsNullOrEmpty(ret) && ret.Length == 36) {
                Authorization = ret;
            }
            else {
                throw new WebServiceException(Errors.ERROR_CREDENTIALS_INCORRECT);
            }

            return ret;
        }

        protected T GetUser<T>() {
            try {
                T user = Get<T>("Authorization", true);
                if (user == null)
                    throw new WebServiceException(Errors.ERROR_CREDENTIALS_INVALID);

                return user;
            }
            catch (Exception ex) {
                throw new WebServiceException(ex.ToString());
            }
        }

        protected IEnumerable<T> GetList<T>(string controller, string msg) {
            try {
                FotoShoutUtils.Log.LogManager.Debug(_logger, msg);
                IEnumerable<T> lst = GetList<T>(controller, true);
                FotoShoutUtils.Log.LogManager.Debug(_logger, Constants.INFO_SUCCEEDED);

                return lst;
            }
            catch (Exception ex) {
                throw new WebServiceException(ex.ToString());
            }
        }

        protected T Get<T>(string path, string msg) {
            try {
                FotoShoutUtils.Log.LogManager.Debug(_logger, msg);
                T obj = Get<T>(path, true);
                FotoShoutUtils.Log.LogManager.Debug(_logger, Constants.INFO_SUCCEEDED);

                return obj;
            }
            catch (Exception ex) {
                throw new WebServiceException(ex.ToString());
            }
        }

        protected T Get<T>(string path, int id, string msg) {
            try {
                FotoShoutUtils.Log.LogManager.Debug(_logger, string.Format(msg, id));
                T obj = Get<T>(path + id.ToString(), true);
                FotoShoutUtils.Log.LogManager.Debug(_logger, Constants.INFO_SUCCEEDED);

                return obj;
            }
            catch (Exception ex) {
                throw new WebServiceException(ex.ToString());
            }
        }

        protected T Upload<T>(string action, T model, string msg, string method = null) {
            FotoShoutUtils.Log.LogManager.Debug(_logger, msg);
            string ret = UploadString(action, JsonConvert.SerializeObject(model), method);
            try {
                FotoShoutUtils.Log.LogManager.Debug(_logger, Constants.INFO_SUCCEEDED);
                return (string.IsNullOrEmpty(ret) ? default(T) : (T)JsonConvert.DeserializeObject(ret));
            }
            catch (Exception ex) {
                throw new WebServiceException(ret + " - " + ex.ToString());
            }
        }

        protected T Modify<T>(string controller, T ev, string msg, string method = null) {
            FotoShoutUtils.Log.LogManager.Debug(_logger, msg);
            string ret = UploadString(controller, JsonConvert.SerializeObject(ev), method);
            try {
                FotoShoutUtils.Log.LogManager.Debug(_logger, Constants.INFO_SUCCEEDED);
                return (string.IsNullOrEmpty(ret) ? default(T) : (T)JsonConvert.DeserializeObject<T>(ret));
            }
            catch (Exception ex) {
                throw new WebServiceException(ret + " - " + ex.ToString());
            }
        }

        protected T Delete<T>(string action, int id, string msg) {
            FotoShoutUtils.Log.LogManager.Debug(_logger, string.Format(msg, id));
            try {
                T obj = Delete<T>(action + "/" + id.ToString(), true);
                FotoShoutUtils.Log.LogManager.Debug(_logger, Constants.INFO_SUCCEEDED);
                return obj;
            }
            catch (Exception ex) {
                throw new WebServiceException(ex.ToString());
            }
        }

        protected abstract Object GenerateCredentials(string apiKey, LoginModel model);
    }
}