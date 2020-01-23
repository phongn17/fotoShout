using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutSyncService {
    internal class AppConfig {
        private static string _fsApiServerBaseAddress = "";
        private static string _fsApiClientBaseAddress = "";

        internal static string FsApiServerBaseAddress {
            get {
                if (string.IsNullOrEmpty(_fsApiServerBaseAddress)) {
                    _fsApiServerBaseAddress = ConfigurationManager.AppSettings[Constants.AS_FS_API_SERVERBASEADDRESS];
                    if (!_fsApiServerBaseAddress.EndsWith("/"))
                        _fsApiServerBaseAddress += "/";
                }

                return _fsApiServerBaseAddress;
            }
        }

        internal static string FsApiClientBaseAddress {
            get {
                if (string.IsNullOrEmpty(_fsApiClientBaseAddress)) {
                    _fsApiClientBaseAddress = ConfigurationManager.AppSettings[Constants.AS_FS_API_CLIENTBASEADDRESS];
                    if (!_fsApiClientBaseAddress.EndsWith("/"))
                        _fsApiClientBaseAddress += "/";
                }

                return _fsApiClientBaseAddress;
            }
        }

        internal static string FsApiPrefix {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_FS_API_PREFIX];
            }
        }

        internal static string FsApiKey {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_FS_API_KEY];
            }
        }

        internal static string FsUserEmail {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_FS_USEREMAIL];
            }
        }

        internal static string FsPassword {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_FS_PASSWORD];
            }
        }

        internal static string ServerConnection {
            get {
                return ConfigurationManager.ConnectionStrings[Constants.CS_SERVERCONNECTION].ConnectionString;
            }
        }
        
        internal static string ClientConnection {
            get {
                return ConfigurationManager.ConnectionStrings[Constants.CS_CLIENTCONNECTION].ConnectionString;
            }
        }

        internal static string SyncAction {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_SYNCACTION];
            }
        }
        
        internal static string EventSyncScopeName {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_EVENTSYNCSCOPENAME];
            }
        }

        internal static string EventSyncDirection {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_EVENTSYNCDIRECTION];
            }
        }

        internal static string[] EventStaticSyncTables {
            get {
                string str = ConfigurationManager.AppSettings[Constants.AS_EVENTSTATICSYNCTABLES];
                return (!string.IsNullOrEmpty(str)) ? str.Split('|') : null;
            }
        }

        internal static string[] EventDynamicSyncTables {
            get {
                string str = ConfigurationManager.AppSettings[Constants.AS_EVENTDYNAMICSYNCTABLES];
                return (!string.IsNullOrEmpty(str)) ? str.Split('|') : null;
            }
        }

        internal static string EventDbServerSyncClass {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_EVENTDBSERVERSYNCCLASS];
            }
        }

        internal static string PhotoSyncScopeName {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_PHOTOSYNCSCOPENAME];
            }
        }

        internal static string PhotoSyncDirection {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_PHOTOSYNCDIRECTION];
            }
        }

        internal static string[] PhotoStaticSyncTables {
            get {
                string str = ConfigurationManager.AppSettings[Constants.AS_PHOTOSTATICSYNCTABLES];
                return (!string.IsNullOrEmpty(str)) ? str.Split('|') : null;
            }
        }

        internal static string[] PhotoDynamicSyncTables {
            get {
                string str = ConfigurationManager.AppSettings[Constants.AS_PHOTODYNAMICSYNCTABLES];
                return (!string.IsNullOrEmpty(str)) ? str.Split('|') : null;
            }
        }

        internal static string PhotoDbServerSyncClass {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_PHOTODBSERVERSYNCCLASS];
            }
        }
        
        internal static string DbClientSyncClass {
            get {
                return ConfigurationManager.AppSettings[Constants.AS_DBCLIENTSYNCCLASS];
            }
        }
    }
}
