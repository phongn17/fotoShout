using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FotoShoutApi.Models;

namespace FotoShoutApi.Controllers
{
    public class EventOptionsTestController : Controller
    {
        //
        // GET: /EventOption/

        public ActionResult Index()
        {
            string apiUrl = Url.HttpRouteUrl("DefaultApi", new { controller = "EventOptions" });
            ViewBag.ApiUrl = new Uri(Request.Url, apiUrl).AbsoluteUri.ToString();

            return View();
        }
   }
}