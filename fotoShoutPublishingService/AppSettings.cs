using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutPublishingService {
    internal class AppSettings {
        private const int PUBLISHDELAY_DEFAULT = 10;
        private static string _fsApiBaseAddress = "";
        
        internal static string FsApiBaseAddress {
            get {
                if (string.IsNullOrEmpty(_fsApiBaseAddress)) {
                    _fsApiBaseAddress = ConfigurationManager.AppSettings[Constants.AS_FS_API_BASEADDRESS];
                    if (!_fsApiBaseAddress.EndsWith("/"))
                        _fsApiBaseAddress += "/";
                }

                return _fsApiBaseAddress;
            }
        }

        internal static string FsApiPrefix {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_FS_API_PREFIX];
            }
        }

        internal static int PublishDelay {
            get {
                string delayStr = ConfigurationManager.AppSettings[Constants.APP_SETTINGS_PUBLISHDELAY];
                if (!string.IsNullOrEmpty(delayStr)) {
                    int delay;
                    if (int.TryParse(delayStr.Trim(), out delay))
                        return delay;
                }

                return AppSettings.PUBLISHDELAY_DEFAULT;
            }
        }
    }
}
