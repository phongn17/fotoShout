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
using FotoShoutData.Models;
using log4net;

namespace FotoShoutApi.Controllers {
    public class EventOptionsController : FotoShoutController {
        static ILog _logger = LogManager.GetLogger(typeof(EventOptionsController));

        private FotoShoutDbContext db = new FotoShoutDbContext();

        // GET api/EventOptions
        public IEnumerable<EventOption> GetEventOptions() {
            try {
                return db.EventOptions.OrderBy(evo => evo.EventOptionName).AsEnumerable();
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = ex.Message });
            }
        }

        // GET api/EventOption/5
        public EventOption GetEventOption(int id) {
            EventOption eventoption = db.EventOptions.Find(id);
            if (eventoption == null)
                this.GenerateException(HttpStatusCode.NotFound, string.Format("An event option with the {0} id not found.", id), _logger);

            return eventoption;
        }

        // PUT api/EventOption/5
        public HttpResponseMessage PutEventOption(int id, EventOption eventoption) {
            if (ModelState.IsValid) {
                if (id != eventoption.EventOptionId)
                    this.GenerateException(HttpStatusCode.BadRequest, "The event option identifier does not match the given id.", _logger);

                if (!eventoption.EmailOption)
                    this.GenerateException(HttpStatusCode.NotAcceptable, "Email is mandatory.", _logger);

                var temp = db.EventOptions.Where(eo => eo.EventOptionName == eventoption.EventOptionName && eo.EventOptionId != id).SingleOrDefault();
                if (temp != null)
                    this.GenerateException(HttpStatusCode.Conflict, string.Format("{0} already exists.", eventoption.EventOptionName), _logger);
                
                db.Entry(eventoption).State = EntityState.Modified;

                try {
                    db.SaveChanges();
                }
                catch (Exception ex) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                    this.GenerateException(HttpStatusCode.NotFound, ex.Message);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // POST api/EventOption
        public HttpResponseMessage PostEventOption(EventOption eventoption) {
            if (ModelState.IsValid) {
                if (!eventoption.EmailOption)
                    this.GenerateException(HttpStatusCode.NotAcceptable, "Email is mandatory.", _logger);
                
                var temp = db.EventOptions.Where(eo => eo.EventOptionName == eventoption.EventOptionName).SingleOrDefault();
                if (temp != null)
                    this.GenerateException(HttpStatusCode.Conflict, string.Format("{0} already exists.", eventoption.EventOptionName), _logger);
                
                db.EventOptions.Add(eventoption);
                try {
                    db.SaveChanges();
                }
                catch (Exception ex) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                    this.GenerateException(HttpStatusCode.NotFound, ex.Message);
                }

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, eventoption);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = eventoption.EventOptionId }));
                return response;
            }
            else {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/EventOption/5
        public HttpResponseMessage DeleteEventOption(int id) {
            EventOption eventoption = db.EventOptions.Find(id);
            if (eventoption == null) {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.EventOptions.Remove(eventoption);

            try {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.NotFound, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, eventoption);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}