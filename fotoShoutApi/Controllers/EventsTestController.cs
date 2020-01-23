using FotoShoutApi.Models;
using FotoShoutData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutApi.Controllers
{
    public class EventsTestController : Controller
    {
        private EventsController apiEvents = new EventsController();
        private FotoShoutDbContext db = new FotoShoutDbContext();
        //
        // GET: /Events/

        public ActionResult Index()
        {
            string apiUri = Url.HttpRouteUrl("DefaultApi", new { controller = "Events" });
            ViewBag.ApiUrl = new Uri(Request.Url, apiUri).AbsoluteUri;

            return View();
        }

        [OutputCache(Duration=0)]
        public ActionResult CreateEvent() {
            return PartialView("CreateEvent", new EventTDO {
                EventOptions = (from evo in db.EventOptions orderby evo.EventOptionName select new EventOptionTDO { EventOptionId = evo.EventOptionId, EventOptionName = evo.EventOptionName }).ToList(),
                Sponsors = (from s in db.Sponsors select new SponsorTDO { SponsorId = s.SponsorId, SponsorName = s.SponsorName }).ToList()
            });
        }

        [OutputCache(Duration = 0)]
        public ActionResult EditEvent(int id) {
            return PartialView("EditEvent", apiEvents.GetEvent(id));
        }
        
        public ActionResult AnnotatePhotos(int id, bool reAnnotate = false) {
            ViewBag.EventId = id.ToString();
            ViewBag.Reannotate = reAnnotate;

            return View();
        }

        public ActionResult Guests(int id) {
            ViewBag.PhotoId = id.ToString();

            return PartialView("PhotoGuests");
        }

        public ActionResult SelectGuests(int id)
        {
            ViewBag.EventId = id.ToString();

            return View();
        }
        
    }
}
