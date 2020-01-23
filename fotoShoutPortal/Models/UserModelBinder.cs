using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace FotoShoutPortal.Models {
    public class UserModelBinder<T> : IModelBinder {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
            if (bindingContext.Model != null)
                throw new InvalidOperationException("Cannot update instances");

            if (controllerContext.RequestContext.HttpContext.Request.IsAuthenticated) {
                var cookie = controllerContext.RequestContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (cookie == null)
                    return null;

                var decrypted = FormsAuthentication.Decrypt(cookie.Value);

                if (!string.IsNullOrEmpty(decrypted.UserData))
                    return JsonConvert.DeserializeObject(decrypted.UserData);
            }

            return null;
        }
    }
}