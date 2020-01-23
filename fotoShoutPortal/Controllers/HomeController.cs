using FotoShoutPortal.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutPortal.Controllers {
    [Authorize]
    public class HomeController : AuthenticatedController {
        static ILog _logger = LogManager.GetLogger(typeof(HomeController));
        
        public ActionResult Index() {
            return View();
        }
    }
}
