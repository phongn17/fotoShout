using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace FotoShoutPortal.Infrastructure {
    public static class HttpResponseBaseExtensions {
        public static int SetAuthCookie<T>(this HttpResponseBase response, string name, bool rememberMe, T userData) {
            // In order to pick up the settings from config, we create a default cookie and use its value to create a new one
            var cookie = FormsAuthentication.GetAuthCookie(name, rememberMe);
            var ticket = FormsAuthentication.Decrypt(cookie.Value);

            var newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate, ticket.Expiration, ticket.IsPersistent, JsonConvert.SerializeObject(userData), ticket.CookiePath);
            var encTicket = FormsAuthentication.Encrypt(newTicket);

            // Use existing cookie
            cookie.Value = encTicket;
            response.Cookies.Add(cookie);

            return encTicket.Length;
        }
    }
}