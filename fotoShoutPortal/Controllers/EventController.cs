using FotoShoutData.Models;
using FotoShoutPortal.Models;
using FotoShoutUtils;
using FotoShoutUtils.Service;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace FotoShoutPortal.Controllers {
    [Authorize]
    public class EventController : AuthenticatedController {
        const string OPER_EVENTS = "Events";
        const string OPER_CREATEEVENT = "CreateEvent";
        const string OPER_REVIEWEVENT = "ReviewEvent";
        const string OPER_EDITEVENT = "EditEvent";
        const string OPER_DELETEEVENT = "DeleteEvent";

        const string OPER_VIEW_REVIEW = "Review";
        const string OPER_VIEW_MODIFY = "Modify";
        
        static ILog _logger = LogManager.GetLogger(typeof(AccountController));

        //
        // GET: /Event/

        public ActionResult Index(string operationName = "", object routeValues = null) {
            switch (operationName) {
                case EventController.OPER_CREATEEVENT:
                    return RedirectToAction(operationName);

                default:
                    return RedirectToAction(string.IsNullOrEmpty(operationName) ? EventController.OPER_EVENTS : operationName, routeValues);

            }
        }

        public ActionResult CreateEvent() {
            return ProcessEvent(EventController.OPER_VIEW_MODIFY, 0);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEvent(EventTDO ev) {
            if (ModelState.IsValid) {
                try {
                    ev.EventFolder = ev.EventFolder.Trim();
                    if (fsWebService.CreateEvent(ev) != null)
                        return RedirectToAction("Events");
                    else {
                        this.AddModelError(string.Format(Errors.ERROR_EVENT_CREATE_UNEXPECTED, ev.EventName), _logger);
                    }
                }
                catch (Exception ex) {
                    GenerateErrorMessage(Constants.ACTION_EVENT_MODIFY, ex, ev);
                }
            }
            else {
                this.AddModelError(ModelState.Values.SelectMany(v => v.Errors).ToList(), _logger);
            }

            InitParameters(ev);

            return View(EventController.OPER_VIEW_MODIFY, ev);
        }

        public ActionResult ReviewEvent(int id) {
            return ProcessEvent(EventController.OPER_VIEW_REVIEW, id);
        }

        public ActionResult EditEvent(int id) {
            ViewBag.StartedSubmitting = fsWebService.GetNumProcessedPhotos(id) != 0;
            return ProcessEvent(EventController.OPER_VIEW_MODIFY, id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEvent(EventTDO ev) {
            if (ModelState.IsValid) {
                try {
                    ev.EventFolder = ev.EventFolder.Trim();
                    if (fsWebService.UpdateEvent(ev.EventId, ev) != null)
                        return RedirectToAction("Events");
                    else {
                        this.AddModelError(string.Format(Errors.ERROR_EVENT_UPDATE_UNEXPECTED, ev.EventName), _logger);
                    }
                }
                catch (Exception ex) {
                    GenerateErrorMessage(Constants.ACTION_EVENT_MODIFY, ex, ev);
                }
            }
            else {
                this.AddModelError(ModelState.Values.SelectMany(v => v.Errors).ToList(), _logger);
            }

            InitParameters(ev);
            ViewBag.StartedSubmitting = fsWebService.GetNumProcessedPhotos(ev.EventId) != 0;

            return View(EventController.OPER_VIEW_MODIFY, ev);
        }

        public ActionResult Events(string eventType) {
            IEnumerable<EventTDO> events = fsWebService.GetEvents(eventType);
            return View("Events", events);
        }

        public ActionResult PartialEvents(string eventType) {
            ViewBag.Error = string.Empty;
            IEnumerable<EventTDO> events = fsWebService.GetEvents(eventType);
            return PartialView("_EventsPartial", events);
        }

        //
        // GET: /Event/CheckEventOptions
        public ActionResult CheckEventOptions() {
            IEnumerable<EventOption> eos = fsWebService.GetEventOptions();
            if (eos == null || eos.Count() == 0)
                return PartialView("_PreventCreation");

            return PartialView("_SubmitCommand");
        }

        //
        // GET: /Event/CompleteEvent/5
        public ActionResult CompleteEvent(int id = 0) {
            EventTDO ev = fsWebService.GetEvent(id);
            return PartialView("_CompleteConfirmation", ev);
        }

        //
        // POST: /Event/CompleteEvent/5
        [HttpPost, ActionName("CompleteEvent")]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteConfirmed(int id) {
            try {
                HttpResponseMessage response = fsWebService.ChangeEventStatus(id, "Completed");
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                ViewBag.Error = ex.Message;
            }

            return Events("Open");
        }

        //
        // GET: /Event/DeleteEvent/5
        public ActionResult DeleteEvent(int id = 0) {
            EventTDO ev = fsWebService.GetEvent(id);
            int numProcessedPhotos = fsWebService.GetNumProcessedPhotos(id);
            if (numProcessedPhotos != 0)
                return PartialView("_PreventDeletion", ev);
            else
                return PartialView("_DeleteConfirmation", ev);
        }

        //
        // POST: /Event/DeleteEvent/5
        [HttpPost, ActionName("DeleteEvent")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) {
            try {
                EventTDO ev = fsWebService.DeleteEvent(id);
                if (ev == null) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, string.Format(Errors.ERROR_EVENT_DELETE, id));
                    ViewBag.Error = string.Format(Errors.ERROR_EVENT_DELETE, id);
                }
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                ViewBag.Error = ex.Message;
            }

            return Events("Open");
        }

        #region Helpers
        private ActionResult ProcessEvent(string viewName, int id) {
            try {
                EventTDO ev = fsWebService.GetEvent(id);
                if (ev != null) {
                    ev.ChannelGroups = GetChannelGroups();
                    ev.Websites = fsWebService.GetEventWebsites();
                    return View(viewName, ev);
                }

                return (id != 0) ? HttpNotFound(string.Format(Errors.ERROR_EVENT_DETAIL, id)) : HttpNotFound(Errors.ERROR_EVENT_EVENTOPTIONS);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                return HttpNotFound(ex.Message);
            }
        }

        private IEnumerable<ChannelGroupTDO> GetChannelGroups() {
            try {
                return (publishWebService != null) ? publishWebService.GetChannelGroups() : new HashSet<ChannelGroupTDO>();
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                return new HashSet<ChannelGroupTDO>();
            }
        }
        
        private void InitParameters(EventTDO ev) {
            ev.ChannelGroups = GetChannelGroups();
            EventTDO temp = fsWebService.GetEvent(0);
            if (temp != null) {
                ev.EventOptions = temp.EventOptions;
                ev.Sponsors = temp.Sponsors;
                ev.EmailTemplates = temp.EmailTemplates;
            }
        }

        private void GenerateErrorMessage(string action, Exception ex, Object data) {
            if (ex.Message.EndsWith("Conflict.")) {
                if (action.Equals(Constants.ACTION_EVENT_MODIFY, StringComparison.InvariantCultureIgnoreCase))
                    this.AddModelError(string.Format(Errors.ACTION_EVENT_CREATE_CONFLICT, ((EventTDO)data).EventName), _logger);
            }
            else {
                this.AddModelError(ex.Message, _logger, ex);
            }
        }

        #endregion // Helpers
    }
}
