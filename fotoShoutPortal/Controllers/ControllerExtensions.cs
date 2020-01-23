using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutPortal.Controllers {
    public static class ControllerExtensions {
        public static void AddModelError(this Controller controller, object messenger, ILog logger = null, Exception ex = null) {
            if (controller.ModelState != null)
                controller.ModelState.AddModelError("", ControllerExtensions.GenerateMessage(messenger));
            if (logger != null)
                FotoShoutUtils.Log.LogManager.Error(logger, ControllerExtensions.GenerateMessage(messenger) + ((ex != null) ? (" - " + ex.ToString()) : ""));
        }

        public static void AddMessage(this Controller controller, object messenger) {
            controller.ViewBag.Message = ControllerExtensions.GenerateMessage(messenger);
        }

        private static string GenerateMessage(object messenger) {
            StringBuilder sb = new StringBuilder();
            if (messenger is List<ModelError>) {
                foreach (ModelError error in (IEnumerable<ModelError>)messenger) {
                    if (sb.Length > 0)
                        sb.Append("\r\n");
                    sb.Append(error.ErrorMessage);
                }
            }
            else
                sb.Append(messenger.ToString());

            return sb.ToString();
        }
    }
}
