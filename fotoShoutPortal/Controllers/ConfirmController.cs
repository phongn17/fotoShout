using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutPortal.Controllers {
    public class ConfirmController : AuthenticatedController {
        //
        // GET: /Confirm/ServiceTerm
        public ActionResult ServiceTerm(string filename) {
            return PartialView("_ServiceTerm", filename);
        }
    }
}
