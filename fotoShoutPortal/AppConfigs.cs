using FotoShoutUtils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FotoShoutPortal {
    public class AppConfigs {
        private static string _apiBaseAddress = "";
        
        internal static string ApiBaseAddress {
            get {
                if (string.IsNullOrEmpty(_apiBaseAddress)) {
                    _apiBaseAddress = ConfigurationManager.AppSettings[Constants.AS_API_BASEADDRESS];
                    if (!_apiBaseAddress.EndsWith("/"))
                        _apiBaseAddress += "/";
                }

                return _apiBaseAddress;
            }
        }

        internal static string ApiPrefix {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_API_PREFIX];
            }
        }

        internal static string ApiKey {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_API_KEY];
            }
        }

        internal static string C9ApiKey {
            get {
                return ConfigurationManager.AppSettings["C9ApiKey"];
            }
        }

        internal static string UserName {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_USERNAME];
            }
        }
        
        internal static string Password {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_PASSWORD];
            }
        }
    }
}