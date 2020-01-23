using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FotoShoutApi.Models;
using log4net;
using FotoShoutApi.Services;
using FotoShoutData.Models;

namespace FotoShoutApi.Controllers
{
    public class EventBroadcastsController : FotoShoutController {
        static ILog _logger = LogManager.GetLogger(typeof(EventBroadcastsController));
        
        private FotoShoutDbContext db = new FotoShoutDbContext();

        // GET api/EventBroadcasts/5
        public IEnumerable<EventBroadcast> GetEventBroadcasts(int id) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return EventBroadcastsService.GetBroadcasts(ev, db);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.Message));
            }
        }

        // PUT api/EventBroadcasts/5
        public HttpResponseMessage PutEventBroadcast(int broadcastId, EventBroadcast eventBroadcast) {
            if (!ModelState.IsValid)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest, ModelState));

            if (broadcastId != eventBroadcast.EventBroadcastId) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format(Errors.ERROR_BROADCAST_UPDATE_IDSNOTIDENTICAL, broadcastId, eventBroadcast.EventBroadcastId));
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest, string.Format(Errors.ERROR_BROADCAST_UPDATE_IDSNOTIDENTICAL, broadcastId, eventBroadcast.EventBroadcastId)));
            }

            db.Entry(eventBroadcast).State = EntityState.Modified;

            try {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.Message));
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/EventBroadcasts
        public HttpResponseMessage PostEventBroadcast(EventBroadcast eventBroadcast) {
            if (ModelState.IsValid && eventBroadcast != null) {
                db.EventBroadcasts.Add(eventBroadcast);

                try {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.Message));
                }

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, eventBroadcast);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { controller = "EventBroadcasts", id = eventBroadcast.EventId,  broadcastId = eventBroadcast.EventBroadcastId }));
                return response;
            }
            else {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest, ModelState.Values.SelectMany(v => v.Errors)));
            }
        }

        // DELETE api/EventBroadcasts/5
        public HttpResponseMessage DeleteEventBroadcast(int broadcastId)
        {
            EventBroadcast eventbroadcast = db.EventBroadcasts.Find(broadcastId);
            if (eventbroadcast == null) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("A broadcast with the id of {0} not found.", broadcastId));
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.EventBroadcasts.Remove(eventbroadcast);

            try {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, eventbroadcast);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}