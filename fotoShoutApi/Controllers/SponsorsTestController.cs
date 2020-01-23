using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutApi.Controllers
{
    public class SponsorsTestController : Controller
    {
        //
        // GET: /Sponsors/

        public ActionResult Index()
        {
            string apiUri = Url.HttpRouteUrl("DefaultApi", new { controller = "Sponsors" });
            ViewBag.ApiUrl = new Uri(Request.Url, apiUri).AbsoluteUri;
            
            return View();
        }

    }
}
