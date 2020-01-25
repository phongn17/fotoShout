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
using Newtonsoft.Json;
using System.Collections;
using System.Web.Routing;
using FotoShoutApi.Utils.IO;
using FotoShoutApi.Services;
using System.Data.Entity.Validation;
using FotoShoutApi.Utils;
using FotoShoutData.Models;
using FotoShoutUtils;
using FotoShoutUtils.Log;

namespace FotoShoutApi.Controllers
{
    public class PhotoAnnotationController : FotoShoutController
    {
        static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(PhotoAnnotationController));
        private FotoShoutDbContext db = new FotoShoutDbContext();

        // id to Guid
        // GET api/PhotoAnnotation/1
        public PhotoAnnotation GetPhotoAnnotation(Guid id) {
            Photo photo = db.Photos.Where(p => p.PhotoId == id).Include(p => p.Event.EventOption).SingleOrDefault();
            if (photo == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, string.Format("Could not found any photo with the {0} id.", id)));

            try {
                return PhotoAnnotationService.GetPhotoAnnotation(photo);
            }
            catch (Exception ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.ToString()));
            }
        }

        // id to Guid
        // PUT api/PhotoAnnotation/1
        public HttpResponseMessage PutPhotoAnnotation(Guid id, PhotoAnnotation photoAnnotation) {
            if (!ModelState.IsValid)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest, ModelState.Values.SelectMany(v => v.Errors)));

            try {
                // Check that all constrains are met
                PhotoAnnotationService.CheckConstrains(photoAnnotation, true);

                Photo photo = db.Photos.Where(p => p.PhotoId == id).Include(p => p.Event.EventOption).SingleOrDefault();
                bool successed = false;
                if (photo != null) {
                    if (photo.Status == (byte)PhotoStatus.Submitted)
                        successed = PhotoAnnotationService.ReannotatePhoto(photo, photoAnnotation, db);
                    else
                        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("The photo status is {0}, so it is invalid for this request.", Enum.GetName(typeof(PhotoStatus), photo.Status))));
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, "Photo not found."));

                if (successed) {
                    db.SaveChanges();
                    // Restore the photo back to the event's folder if its status is back to Unselected
                    if (photo.Status == (byte)PhotoStatus.Unselected)
                        PhotoAnnotationService.RestorePhoto(photo.Folder, Constants.STR_PROCESSED, photo.Filename);
                }
            }
            catch (DbUpdateConcurrencyException ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.ToString()));
            }
            catch (DbEntityValidationException ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.EntityValidationErrors));
            }
            catch (ArgumentNullException ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NoContent, ex.Message));
            }
            catch (Exception ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()));
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // id to Guid
        // POST api/PhotoAnnotation/1
        public HttpResponseMessage PostPhotoAnnotation(Guid id, PhotoAnnotation photoAnnotation) {
            if (!ModelState.IsValid)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest, ModelState.Values.SelectMany(v => v.Errors)));

            try {
                // Check that all constrains are met
                PhotoAnnotationService.CheckConstrains(photoAnnotation, false);
                
                Photo photo = db.Photos.Where(p => p.PhotoId == id).Include(p => p.Event.EventOption).SingleOrDefault();
                bool succeeded = false;
                if (photo != null) {
                    if (photo.Status == (byte)PhotoStatus.Selected) {
                        LogManager.Info(PhotoAnnotationController._logger, string.Format("Annotating the photo {0} with the option {1} ...", id, photo.Event.EventOption.EventOptionId));
                        succeeded = PhotoAnnotationService.AnnotatePhoto(photo, photoAnnotation, db);
                        LogManager.Info(PhotoAnnotationController._logger, string.Format("Annotated the photo {0} ...", id));
                    }
                    else if (photo.Status == (byte)PhotoStatus.Unselected)
                        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, "The photo need to be selected prior to be assigned to guests."));
                    else
                        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, string.Format("The photo status is {0}, so it can not be annotated.", Enum.GetName(typeof(PhotoStatus), photo.Status))));
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, "Photo not found."));

                if (succeeded) {
                    photo.Submitted = DateTime.Now;
                    photo.SubmittedBy = CurrentUser.Id;
                    //LogManager.Info(PhotoAnnotationController._logger, string.Format("Saving the photo {0} ...", id));
                    db.SaveChanges();
                    //LogManager.Info(PhotoAnnotationController._logger, string.Format("Saved the photo {0} ...", id));
                    // Move the photo to the Processed sub-folder
                    if (photo.Status == (byte)PhotoStatus.Submitted) {
                        //LogManager.Info(PhotoAnnotationController._logger, string.Format("Moving the photo {0} ...", id));
                        PhotoAnnotationService.MovePhoto(photo.Folder, Constants.STR_PROCESSED, photo.Filename);
                        //LogManager.Info(PhotoAnnotationController._logger, string.Format("Moved the photo {0} ...", id));
                    }
                }
            }
            catch (DbUpdateConcurrencyException ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex.ToString()));
            }
            catch (DbEntityValidationException ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.EntityValidationErrors));
            }
            catch (ArgumentNullException ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NoContent, ex.Message));
            }
            catch (Exception ex) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString()));
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}