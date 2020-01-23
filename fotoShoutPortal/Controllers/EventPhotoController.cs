using FotoShoutData.Models;
using FotoShoutUtils.Actions;
using FotoShoutUtils.Utils.IO;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace FotoShoutPortal.Controllers {
    [Authorize]
    public class EventPhotoController : AuthenticatedController {
        const string OPER_UNPROCESSED_PHOTOS = "Photos";
        const string OPER_PROCESSED_PHOTOS = "History";
        const string OPER_PROCESSED_PHOTOS_REPORT = "Report";

        const string PARTIALVIEW_PROCESSED = "_EventProcessedPhotos";
        const string PARTIALVIEW_UNPROCESSED = "_EventUnprocessedPhotos";
        
        static ILog _logger = LogManager.GetLogger(typeof(EventPhotoController));

        //
        // GET: /EventPhoto/Details/5
        public ActionResult Details(int id, string viewType = "")
        {
            try {
                if (UpdateEventInfo(id)) {
                    if (string.IsNullOrEmpty(viewType) || viewType.Equals(EventPhotoController.OPER_UNPROCESSED_PHOTOS, StringComparison.InvariantCultureIgnoreCase)) {
                        viewType = EventPhotoController.OPER_UNPROCESSED_PHOTOS;
                        IEnumerable<PhotoTDO> photos = GetPhotos(id, viewType);
                        return View(photos);
                    }
                    else {
                        IEnumerable<PhotoGroupTDO> photoGroups = fsWebService.GetProcessedPhotoGroupsByDate(id);
                        return View(photoGroups);
                    }
                }

                return View((object)null);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                return HttpNotFound(ex.Message);
            }
        }

        //
        // GET: /EventPhoto/ProcessedPhotos/5?created='01/01/2013'
        public ActionResult PartialProcessedPhotos(int id, string created, int page = 0, int pageSize = 12) {
            try {
                if (UpdateEventInfo(id)) {
                    IEnumerable<PhotoTDO> photos = GetPhotos(id, EventPhotoController.OPER_PROCESSED_PHOTOS, created, page, pageSize);
                    return PartialView("_ProcessedPhotos", photos);
                }
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                return PartialView("_Error", ex.ToString());
            }

            return PartialView("_ProcessedPhotos", null);
        }

        public ActionResult Report(int id) {
            try {
                EventDetailsTDO photosDetails = fsWebService.GetProcessedPhotosReporting(id);
                return PartialView("_EventProcessedPhotosReport", photosDetails);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                return PartialView("_Error", ex.ToString());
            }
        }

        //
        // GET: /EventPhoto/ExportToExcel/<filename>
        public ActionResult ExportToExcel(int id) {
            try {
                EventDetailsTDO photosDetails = fsWebService.GetProcessedPhotosReporting(id);
                return new ExportResult<EventDetailsTDO>("~/Views/EventPhoto/_EventProcessedPhotosExport.cshtml", "ExportPhotosList.csv", photosDetails);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                return PartialView("_Error", ex.ToString());
            }
        }

        public ActionResult Error(string msg) {
            return PartialView("_Error", msg);
        }
        
        public ActionResult PartialPhotos(int id, string viewType) {
            try {
                if (UpdateEventInfo(id)) {
                    if (viewType.Equals(EventPhotoController.OPER_PROCESSED_PHOTOS, StringComparison.InvariantCultureIgnoreCase)) {
                        IEnumerable<PhotoGroupTDO> photoGroups = fsWebService.GetProcessedPhotoGroupsByDate(id);
                        return PartialView(EventPhotoController.PARTIALVIEW_PROCESSED, photoGroups);
                    }

                    IEnumerable<PhotoTDO> photos = GetPhotos(id, viewType);
                    if (viewType.Equals(EventPhotoController.OPER_UNPROCESSED_PHOTOS, StringComparison.InvariantCultureIgnoreCase))
                        return PartialView(EventPhotoController.PARTIALVIEW_UNPROCESSED, photos);
                    
                    ViewBag.Error = string.Format("The view type {0} is not supported.", viewType);
                    FotoShoutUtils.Log.LogManager.Error(_logger, ViewBag.Error);
                }
            }
            catch (Exception ex) {
                ViewBag.Error = ex.Message;
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
            }
            return PartialView(EventPhotoController.PARTIALVIEW_UNPROCESSED, null);
        }

        private bool UpdateEventInfo(int id) {
            EventTDO ev = fsWebService.GetEvent(id);
            if (ev != null) {
                ViewBag.EventName = ev.EventName;
                ViewBag.EventDate = ev.EventDate.ToString("dddd MM/dd/yyyy");
                return true;
            }
                
            ViewBag.Error = "Unexpected error: An event with the id of {0} not found.";
            return false;
        }
        
        private IEnumerable<PhotoTDO> GetPhotos(int id, string viewType, string created = null, int page = 0, int pageSize = 12) {
            IEnumerable<PhotoTDO> photos = null;
            if (viewType.Equals(EventPhotoController.OPER_UNPROCESSED_PHOTOS, StringComparison.InvariantCultureIgnoreCase))
                photos = fsWebService.GetPhotos(id);
            else 
                photos = fsWebService.GetProcessedPhotosDetailing(id, created, page, pageSize);
            
            return photos;
        }

    }
}
