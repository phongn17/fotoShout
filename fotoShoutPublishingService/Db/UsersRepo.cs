using FotoShoutData.Models;
using FotoShoutData.Models.Authenticate;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutPublishingService.Db {
    internal class UsersRepo {
        private static ILog _logger = LogManager.GetLogger(typeof(UsersRepo));

        private static string ConnectionString {
            get {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["ServerConnection"];
                string connectStr = (settings != null) ? settings.ConnectionString : null;
                if (string.IsNullOrEmpty(connectStr))
                    throw new ArgumentNullException("ServerConnection");

                return connectStr;
            }
        }

        public static IEnumerable<Credentials> Credentials {
            get {
                IEnumerable<Credentials> credentials = null;
                string connectStr = UsersRepo.ConnectionString;
                using (var dc = new FotoShoutDbDataContext(connectStr)) {
                    credentials = dc.Users.Join(dc.Accounts, u => u.Account_Id, a => a.Id,
                        (u, a) => new Credentials {
                            APIKey = a.ApiKey.ToString(),
                            Email = u.Email,
                            Password = u.Password
                        }).ToList();
                }

                return credentials;
            }
        }
    }
}
