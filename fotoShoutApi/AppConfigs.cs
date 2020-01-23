using FotoShoutApi.Utils;
using FotoShoutUtils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FotoShoutApi {
    public class AppConfigs {
        private static string _virtualRoot = "";

        internal static string VirtualRoot {
            get {
                if (string.IsNullOrEmpty(_virtualRoot)) {
                    _virtualRoot = ConfigurationManager.AppSettings[Constants.AS_VIRTUALDIRROOT];
                    if (string.IsNullOrEmpty(_virtualRoot))
                        _virtualRoot = "";
                    else if (!_virtualRoot.EndsWith("/"))
                        _virtualRoot += "/";
                }

                return _virtualRoot;
            }

            set {
                _virtualRoot = value;
            }
        }
    }
}