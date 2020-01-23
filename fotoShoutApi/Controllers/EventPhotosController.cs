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
using System.Dynamic;
using FotoShoutApi.Utils;
using FotoShoutApi.Services;
using System.IO;
using FotoShoutApi.Utils.IO;
using System.ComponentModel;
using FotoShoutData.Models;
using FotoShoutUtils;
using FotoShoutUtils.Log;

namespace FotoShoutApi.Controllers
{
    public class EventPhotosController : FotoShoutController
    {
        static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(EventPhotosController));
        
        private FotoShoutDbContext db = new FotoShoutDbContext();

        // GET api/EventPhotos/Photos/1
        [HttpGet]
        [ActionName("Photos")]
        public IEnumerable<PhotoTDO> GetPhotos(int id, int page = 0, int pageSize = 12)
        {
            LogManager.Info(EventPhotosController._logger, string.Format("Getting photos of the event {0}...", id));
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db, false, false);
            try {
                return EventPhotosService.GetPhotos(ev);
            }
            catch (Exception ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.Message));
            }
        }

        // GET api/EventPhotos/NumUnclaimedPhotos/1
        [HttpGet]
        [ActionName("NumUnclaimedPhotos")]
        public int GetNumUnclaimedPhotos(int id) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return EventPhotosService.GetNumUnclaimedPhotos(ev);
            }
            catch (Exception ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.Message));
            }
        }

        // GET api/EventPhotos/UnclaimedPhotos/1?page=1&pageSize=12
        [HttpGet]
        [ActionName("UnclaimedPhotos")]
        public IEnumerable<PhotoTDO> GetUnclaimedPhotos(int id, int page = 0, int pageSize = 12) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return EventPhotosService.GetUnclaimedPhotos(ev, page, pageSize);
            }
            catch (Exception ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.Message));
            }
        }

        // GET api/EventPhotos/SubmittedPhotos/1
        [HttpGet]
        [ActionName("SubmittedPhotos")]
        public IEnumerable<PhotoTDO> GetSubmittedPhotos(int id) {
            LogManager.Info(EventPhotosController._logger, string.Format("Getting submitted photos of the event {0}...", id));
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return EventPhotosService.GetSubmittedPhotos(ev, db);
            }
            catch (Exception ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()));
            }
        }

        // GET api/EventPhotos/SubmittedPhoto/1?name=<guest name>
        [HttpGet]
        [ActionName("SubmittedPhotos")]
        public IEnumerable<PhotoTDO> GetSubmittedPhotosByGuest(int id, string name) {
            LogManager.Info(EventPhotosController._logger, string.Format("Getting submitted photos of the event {0} by {1} ...", id, name));
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db, true, true);
            try {
                return EventPhotosService.GetSubmittedPhotosByGuest(ev, name, db);
            }
            catch (Exception ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()));
            }
        }

        // GET api/EventPhotos/PublishedPhotos/1
        [HttpGet]
        [ActionName("PublishedPhotos")]
        public IEnumerable<PhotoTDO> GetPublishedPhotos(int id) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return EventPhotosService.GetPublishedPhotos(ev, db);
            }
            catch (Exception ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()));
            }
        }

        /// <summary>
        /// Get the list of processed photos by guest
        /// </summary>
        // GET api/EventPhotos/PublishedPhoto/1?name=<guest name>
        [HttpGet]
        [ActionName("PublishedPhotos")]
        public IEnumerable<PhotoTDO> GetPublishedPhotosByGuest(int id, string name) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db, true, true);
            try {
                return EventPhotosService.GetPublishedPhotosByGuest(ev, name, db);
            }
            catch (Exception ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()));
            }
        }

        // GET api/EventPhotos/NumProcessedPhotos/1?created='01%2F01%2F2013'
        [HttpGet]
        [ActionName("NumProcessedPhotos")]
        public int GetNumProcessedPhotos(int id, string created = null) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return EventPhotosService.GetNumProcessedPhotos(ev, created);
            }
            catch (Exception ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.Message));
            }
        }

        // GET api/EventPhotos/ProcessedPhotos/1?created='01%2F01%2F2013'&page=1&pageSize=12
        [HttpGet]
        [ActionName("ProcessedPhotos")]
        public IEnumerable<PhotoTDO> GetProcessedPhotos(int id, string created = null, int page = 0, int pageSize = 12) {
            LogManager.Info(EventPhotosController._logger, string.Format("Getting processed photos of the event {0}...", id));
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return EventPhotosService.GetProcessedPhotos(ev, created, page, pageSize, db);
            }
            catch (Exception ex) {
                LogManager.Error(EventPhotosController._logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()));
            }
        }

        // GET api/EventPhotos/ProcessedPhotosDetailing/1
        [HttpGet]
        [ActionName("ProcessedPhotosDetailing")]
        public IEnumerable<PhotoDetailsTDO> GetProcessedPhotosDetailing(int id, string created = null, int page = 0, int pageSize = 12) {
            LogManager.Info(EventPhotosController._logger, string.Format("Getting details of the processed photos of the event {0}...", id));
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return EventPhotosService.GetProcessedPhotosDetailing(ev, created, page, pageSize, db);
            }
            catch (Exception ex) {
                LogManager.Error(EventPhotosController._logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.Message));
            }
        }

        // GET api/EventPhotos/ProcessedPhotosReporting/1
        [HttpGet]
        [ActionName("ProcessedPhotosReporting")]
        public EventDetailsTDO GetProcessedPhotosReporting(int id) {
            LogManager.Info(EventPhotosController._logger, string.Format("Getting reporting processed photos of the event {0}...", id));
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return EventPhotosService.GetProcessedPhotosReporting(ev, db);
            }
            catch (Exception ex) {
                LogManager.Error(EventPhotosController._logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.Message));
            }
        }

        // GET api/EventPhotos/ProcessedPhotoGroupsByDate/1
        [HttpGet]
        [ActionName("ProcessedPhotoGroupsByDate")]
        public IEnumerable<PhotoGroupTDO> GetProcessedPhotoGroupsByDate(int id) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return EventPhotosService.GetProcessedPhotoGroupsByDate(ev, db);
            }
            catch (Exception ex) {
                LogManager.Error(EventPhotosController._logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()));
            }
        }

        // GET api/EventPhotos/NumProcessedPhotos/1?name=<guest name>&created='01/01/2013'&page=1&pageSize=12
        [HttpGet]
        [ActionName("NumProcessedPhotos")]
        public int GetNumProcessedPhotos(int id, string name, string created = null) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db, true, true);
            try {
                return EventPhotosService.GetNumProcessedPhotosByGuest(ev, name, created);
            }
            catch (Exception ex) {
                LogManager.Error(EventPhotosController._logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.Message));
            }
        }

        // GET api/EventPhotos/ProcessedPhotos/1?name=<guest name>&created='01/01/2013'&page=1&pageSize=12
        [HttpGet]
        [ActionName("ProcessedPhotos")]
        public IEnumerable<PhotoTDO> GetProcessedPhotosByGuest(int id, string name, string created = null, int page = 0, int pageSize = 12) {
            LogManager.Info(EventPhotosController._logger, string.Format("Getting processed photos of the event {0} by {1}...", id, name));
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db, true, true);
            try {
                return EventPhotosService.GetProcessedPhotosByGuest(ev, name, created, page, pageSize, db);
            }
            catch (Exception ex) {
                LogManager.Error(EventPhotosController._logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()));
            }
        }

        // GET api/EventPhotos/PublishAuthorizedPhotos/1
        [HttpGet]
        [ActionName("PublishAuthorizedPhotos")]
        public IEnumerable<PhotoTDO> GetPublishAuthorizedPhotos(int id) {
            LogManager.Info(EventPhotosController._logger, string.Format("Getting authorized photos of the event {0}...", id));
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return EventPhotosService.GetPublishAuthorizedPhotos(ev, db);
            }
            catch (Exception ex) {
                LogManager.Error(EventPhotosController._logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()));
            }
        }

        // GET api/EventPhotos/PublishAuthorizedPhotos/1
        [HttpGet]
        [ActionName("PublishUnauthorizedPhotos")]
        public IEnumerable<PhotoTDO> GetUnauthorizedPhotos(int id) {
            LogManager.Info(EventPhotosController._logger, string.Format("Getting unauthorized photos of the event {0}...", id));
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return EventPhotosService.GetUnauthorizedPhotos(ev, db);
            }
            catch (Exception ex) {
                LogManager.Error(EventPhotosController._logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()));
            }
        }

        // GET api/EventPhotos/Thumbnail/1
        [HttpGet]
        [ActionName("Thumbnail")]
        public string GetThumbnailUrl(int id) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                return AppConfigs.VirtualRoot + EventPhotosService.GetThumbnailUrl(ev.EventId, ev.EventFolder, ev.EventVirtualPath, db);
            }
            catch (Exception ex) {
                LogManager.Error(EventPhotosController._logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()));
            }
        }

        // PUT api/EventPhotos/Select/1?filename=<filename>
        [HttpPut]
        [ActionName("Select")]
        public HttpResponseMessage SelectPhoto(int id, string filename) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            Photo photo = GetPhotoFromId(id)
                .Where(p => p.Filename.Equals(filename, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            if (photo == null) {
                string folder = ev.EventFolder.EndsWith(Path.DirectorySeparatorChar.ToString()) ? ev.EventFolder : (ev.EventFolder + Path.DirectorySeparatorChar);
                string thumbnail = File.Exists(folder + Constants.STR_THUMB + "/" + filename) ? (ev.EventVirtualPath + "/" + Constants.STR_THUMB + "/" + filename) : "";
                photo = new Photo {
                    PhotoId = Guid.NewGuid(),
                    Folder = ev.EventFolder,
                    Filename = filename,
                    Image = ev.EventVirtualPath + '/' + filename,
                    Thumbnail = thumbnail,
                    Created = File.GetCreationTime(folder + filename),
                    Event = ev
                };
                ev.Photos.Add(photo);
            }
            else if (photo.Status == (byte)PhotoStatus.Selected) 
                return Request.CreateResponse(HttpStatusCode.Conflict, string.Format("The {0} photo is editing by someone else.", filename));

            photo.Status = (byte)PhotoStatus.Selected;

            try {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex) {
                LogManager.Error(EventPhotosController._logger, ex.ToString());
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.ToString()));
            }

            return Request.CreateResponse(HttpStatusCode.OK, EventPhotosService.GenerateTDO(ev, photo));
        }

        // PUT api/EventPhotos/Unselect/1?filename=<filename>
        [HttpPut]
        [ActionName("Unselect")]
        public HttpResponseMessage UnselectPhoto(int id, string filename) {
            return UpdatePhotoStatus(id, filename, PhotoStatus.Selected, PhotoStatus.Unselected);
        }

        // PUT api/EventPhotos/Submitted/1?filename=<filename>
        [HttpPut]
        [ActionName("Submitted")]
        public HttpResponseMessage SubmittedPhoto(int id, string filename) {
            return UpdatePhotoStatus(id, filename, PhotoStatus.Selected, PhotoStatus.Submitted);
        }

        // PUT api/EventPhotos/PendingPublish/1?filename=<filename>
        [HttpPut]
        [ActionName("PendingPublish")]
        public HttpResponseMessage PendingPublishPhoto(int id, string filename) {
            return UpdatePhotoStatus(id, filename, PhotoStatus.Submitted, PhotoStatus.PendingPublish);
        }

        // PUT api/EventPhotos/UnpendingPublish/1?filename=<filename>
        [HttpPut]
        [ActionName("UnpendingPublish")]
        public HttpResponseMessage UnpendingPublishPhoto(int id, string filename, string error = "") {
            return UpdatePhotoStatus(id, filename, PhotoStatus.PendingPublish, PhotoStatus.Submitted, error);
        }

        // PUT api/EventPhotos/Published/1?filename=<filename>
        [HttpPut]
        [ActionName("Published")]
        public HttpResponseMessage PublishedPhoto(int id, string filename) {
            return UpdatePhotoStatus(id, filename, PhotoStatus.PendingPublish, PhotoStatus.Published);
        }

        // PUT api/EventPhotos/UnselectAll/1
        [HttpPut]
        [ActionName("UnselectAll")]
        public HttpResponseMessage UnselectAllPhotos(int id) {
            IEnumerable<Photo> photos = GetPhotoFromId(id, PhotoStatus.Selected).AsEnumerable();

            if (photos.Any()) {
                foreach (Photo photo in photos)
                    photo.Status = (byte)PhotoStatus.Unselected;
                
                try {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex) {
                    LogManager.Error(EventPhotosController._logger, ex.ToString());
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.ToString()));
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // PUT api/EventPhotos/Unclaim/1?filename=<filename>
        [HttpPut]
        [ActionName("Unclaim")]
        public HttpResponseMessage UnclaimPhoto(int id, string filename) {
            return UpdateClaimingPhotoStatus(id, filename, PhotoStatus.Unclaimed);
        }

        // PUT api/EventPhotos/Reclaimed/1?filename=<filename>
        [HttpPut]
        [ActionName("Reclaim")]
        public HttpResponseMessage ReclaimPhoto(int id, string filename) {
            return UpdateClaimingPhotoStatus(id, filename, PhotoStatus.Unselected);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private IQueryable<Photo> GetPhotoFromId(int id) {
            return db.Photos.Where(p => p.Event.EventId == id);
        }
        
        private IQueryable<Photo> GetPhotoFromId(int id, PhotoStatus status) {
            return GetPhotoFromId(id).Where(p => p.Status == (byte)status);
        }

        private HttpResponseMessage UpdatePhotoStatus(int id, string filename, PhotoStatus condStatus, PhotoStatus status, string error = "") {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            HttpResponseMessage response = null;
            try {
                response = DoUpdatePhotoStatus(id, filename, condStatus, status, error);
                if (response.StatusCode == HttpStatusCode.OK)
                    db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.InternalServerError, ex.Message);
            }

            return response;
        }

        private HttpResponseMessage UpdateClaimingPhotoStatus(int id, string filename, PhotoStatus status) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {    
                if (status == PhotoStatus.Unclaimed)
                    PhotoAnnotationService.MovePhoto(ev.EventFolder, Constants.STR_UNCLAIMED, filename);
                else if (status == PhotoStatus.Unselected)
                    PhotoAnnotationService.RestorePhoto(ev.EventFolder, Constants.STR_UNCLAIMED, filename);
                
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.ToString()));
            }
        }
        
        private HttpResponseMessage DoUpdatePhotoStatus(int id, string filename, PhotoStatus condStatus, PhotoStatus status, string error) {
            Photo photo = GetPhotoFromId(id)
                .Where(p => p.Filename.Equals(filename, StringComparison.InvariantCultureIgnoreCase) && p.Status == (byte)condStatus).SingleOrDefault();
            if (photo == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, string.Format("Unexpected error: The {0} photo not found as {1} in the database.", filename, condStatus.ToString()));

            photo.Error = error;
            photo.Status = (byte)status;
            return Request.CreateResponse(HttpStatusCode.OK);
        }

    }
}