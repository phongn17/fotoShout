using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutApi.Controllers
{
    public class AuthorizationTestController : Controller
    {
        //
        // GET: /AuthorizationTest/

        public ActionResult Index()
        {
            string apiUri = Url.HttpRouteUrl("DefaultApi", new { controller = "Authorization" });
            ViewBag.ApiUrl = new Uri(Request.Url, apiUri).AbsoluteUri;

            return View();
        }

    }
}
