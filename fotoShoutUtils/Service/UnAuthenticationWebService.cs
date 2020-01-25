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
    public class UnAuthenticationWebService: WebClientService {
        readonly static ILog _logger = LogManager.GetLogger(typeof(UnAuthenticationWebService));
        
        public UnAuthenticationWebService(string baseAddress, string prefix, string mediaType) 
            : base(baseAddress, prefix, mediaType) {
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
    }
}