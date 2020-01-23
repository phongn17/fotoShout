using FotoShoutApi.Models;
using FotoShoutApi.Utils;
using FotoShoutData.Models;
using FotoShoutUtils;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace FotoShoutApi.Controllers
{
    public class AuthorizationController : ApiController {
        static ILog _logger = LogManager.GetLogger(typeof(AuthorizationController));
        
        private FotoShoutDbContext db = new FotoShoutDbContext();

        // GET api/Authorization
        public UserTDO Get() {
            UserTDO tdo = null;
            try {
                // get the api key
                string apiKey = this.GetApiKey(_logger);

                FotoShoutUtils.Log.LogManager.Info(_logger, "Getting user info...");
                User user = this.GetUser(apiKey, db);
                if (user == null)
                    this.GenerateException(HttpStatusCode.Unauthorized, Errors.ERROR_AUTHORIZATIONKEY_INVALID, _logger);

                FotoShoutUtils.Log.LogManager.Info(_logger, "Successfully got user info");

                FotoShoutUtils.Log.LogManager.Info(_logger, "Getting account info...");
                Account account = this.GetAccount(user.Id, db);
                if (account == null)
                    this.GenerateException(HttpStatusCode.Unauthorized, Errors.ERROR_AUTHORIZATIONKEY_INVALID, _logger);

                FotoShoutUtils.Log.LogManager.Info(_logger, "Successfully got account info");

                tdo = new UserTDO {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    MiddleInitial = user.MiddleInitial,
                    Title = user.Title,
                    Phone = user.Phone,
                    PhoneExt = user.PhoneExt,
                    Email = user.Email,
                    Status = user.Status,
                    CreatedBy = user.CreatedBy,
                    Created = user.Created,
                    AccountName = account.AccountName,
                    Role = (user.Role != null) ? user.Role.Name : ""
                };
            }
            catch (HttpResponseException ex) {
                throw ex;
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.BadRequest, ex.Message);
            }

            return tdo;
        }

        // GET api/Authorization
        public string GetData(string type) {
            // get the api key
            string apiKey = this.GetApiKey(_logger);
            
            FotoShoutUtils.Log.LogManager.Info(_logger, "Getting user authorization info...");
            UserAuthorization userAuth = (from ua in db.UserAuthorizations where ua.AuthorizationKey == new Guid(apiKey) select ua).SingleOrDefault();
            if (userAuth == null)
                this.GenerateException(HttpStatusCode.Unauthorized, Errors.ERROR_AUTHORIZATIONKEY_INVALID, _logger);

            FotoShoutUtils.Log.LogManager.Info(_logger, "Successfully");

            return (type.Equals("Id", StringComparison.InvariantCultureIgnoreCase) ? userAuth.Id.ToString() : "");
        }

        // POST api/Authorization
        public HttpResponseMessage Post(Credentials credentials) {
            if (credentials == null)
                this.GenerateException(HttpStatusCode.Unauthorized, Errors.ERROR_CREDENTIALS_EMPTY, _logger);

            try {
                FotoShoutUtils.Log.LogManager.Info(_logger, "Logging in...");
                User user = Login(credentials.Email, credentials.Password);
                if (user == null)
                    this.GenerateException(HttpStatusCode.Unauthorized, Errors.ERROR_LOGIN_INVALID, _logger);

                if (user.Account == null)
                    this.GenerateException(HttpStatusCode.Unauthorized, Errors.ERROR_LOGIN_NOACCOUNT, _logger);
                else {
                    // check to see if the API key is valid
                    FotoShoutUtils.Log.LogManager.Info(_logger, "Getting account info that is associated with the API key...");
                    Account account = db.Accounts.SingleOrDefault(a => a.Id == user.Account.Id && a.ApiKey == new Guid(credentials.APIKey));
                    if (account == null)
                        this.GenerateException(HttpStatusCode.Unauthorized, Errors.ERROR_APIKEY_NOTFOUND, _logger);

                    FotoShoutUtils.Log.LogManager.Info(_logger, "Successfully got account info");
                }

                if (user.Authorization == null) {
                    FotoShoutUtils.Log.LogManager.Info(_logger, "Generating user authorization key...");
                    user.Authorization = new UserAuthorization { AuthorizationKey = Guid.NewGuid(), Created = DateTime.Now };
                    db.SaveChanges();
                    FotoShoutUtils.Log.LogManager.Info(_logger, "Successfully generated user authorization key");
                }

                FotoShoutUtils.Log.LogManager.Info(_logger, "Successfully logged in");
                
                return Request.CreateResponse(HttpStatusCode.Created, user.Authorization.AuthorizationKey.ToString());
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = ex.Message, Content = new StringContent(ex.Message) });
            }
        }

        protected override void Dispose(bool disposing) {
            db.Dispose();
            base.Dispose(disposing);
        }

        private User Login(string email, string password) {
            return db.Users.Where(u => u.Email == email && u.Password == password && u.Status.Equals("Active", StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
        }
    }
}
