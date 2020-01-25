using FotoShoutApi.Models;
using FotoShoutApi.Services;
using FotoShoutData.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace FotoShoutApi.Controllers {
    public class PhotoEmailsController : FotoShoutController {
        private readonly static ILog _logger = log4net.LogManager.GetLogger(typeof(EventBroadcastsController));
        private readonly FotoShoutDbContext db = new FotoShoutDbContext();

        public IEnumerable<PhotoEmail> GetPhotoEmails(Guid id) {
            Photo photo = this.db.Photos.Find(id);
            if (photo == null)
                throw new HttpResponseException(Request.CreateResponse<string>(HttpStatusCode.NotFound, string.Format("Could not found any photo with the {0} id.", id)));
            try {
                return PhotoEmailsService.GetPhotoEmails(photo, this.db);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(PhotoEmailsController._logger, (object)ex.ToString());
                throw new HttpResponseException(Request.CreateResponse<string>(HttpStatusCode.NotFound, ex.Message));
            }
        }

        public HttpResponseMessage PutPhotoEmail(int photoEmailId, PhotoEmail photoEmail) {
            if (!ModelState.IsValid)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest, ModelState.Values.SelectMany(v => v.Errors)));

            if (photoEmailId != photoEmail.PhotoEmailId) {
                FotoShoutUtils.Log.LogManager.Error(PhotoEmailsController._logger, (object)string.Format("Ids need to be identical when updating the email status {0} of the photo {1}.", photoEmailId, photoEmail.PhotoId));
                throw new HttpResponseException(Request.CreateResponse<string>(HttpStatusCode.BadRequest, string.Format("Ids need to be identical when updating the email status {0} of the photo {1}.", photoEmailId, photoEmail.PhotoId)));
            }

            db.Entry(photoEmail).State = EntityState.Modified;

            try {
                this.db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex) {
                FotoShoutUtils.Log.LogManager.Error(PhotoEmailsController._logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse<string>(HttpStatusCode.NotFound, ex.Message));
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage PostPhotoEmail(PhotoEmail photoEmail) {
            if (!ModelState.IsValid || photoEmail == null)
                throw new HttpResponseException(Request.CreateResponse<IEnumerable<ModelError>>(HttpStatusCode.BadRequest, this.ModelState.Values.SelectMany(v => v.Errors)));
            
            this.db.PhotoEmails.Add(photoEmail);

            try {
                this.db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex) {
                FotoShoutUtils.Log.LogManager.Error(PhotoEmailsController._logger, (object)((object)ex).ToString());
                throw new HttpResponseException(Request.CreateResponse<string>(HttpStatusCode.NotFound, ex.Message));
            }

            HttpResponseMessage response = Request.CreateResponse<PhotoEmail>(HttpStatusCode.Created, photoEmail);
            response.Headers.Location = new Uri(Url.Link("DefaultApi", new {
                controller = "PhotoEmails",
                id = photoEmail.PhotoId
            }));

            return response;
        }

        public HttpResponseMessage DeleteEventBroadcast(int broadcastId)
        {
            EventBroadcast eventBroadcast = this.db.EventBroadcasts.Find(broadcastId);
            if (eventBroadcast == null) {
                FotoShoutUtils.Log.LogManager.Error(PhotoEmailsController._logger, (object)string.Format("A broadcast with the id of {0} not found.", (object)broadcastId));
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            this.db.EventBroadcasts.Remove(eventBroadcast);

            try {
                this.db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                FotoShoutUtils.Log.LogManager.Error(PhotoEmailsController._logger, ex.ToString());
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, (Exception)ex);
            }
            
            return Request.CreateResponse<EventBroadcast>(HttpStatusCode.OK, eventBroadcast);
        }

        protected override void Dispose(bool disposing) {
            this.db.Dispose();
            base.Dispose(disposing);
        }
    }
}
