using FotoShoutData.Models;
using FotoShoutUtils;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutPortal.Controllers {
    [Authorize]
    public class EventOptionController : AuthenticatedController {
        static ILog _logger = LogManager.GetLogger(typeof(EventOptionController));
        //
        // GET: /EventOption/
        public ActionResult Index() {
            return View(fsWebService.GetEventOptions());
        }

        //
        // GET: /EventOption/Create
        public ActionResult Create() {
            return Modify(0);
        }

        //
        // POST: /EventOption/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EventOption evOption) {
            if (ModelState.IsValid) {
                try {
                    evOption.EmailOption = true;
                    if (fsWebService.CreateEventOption(evOption) != null)
                        return RedirectToAction("Index");
                    else {
                        this.AddModelError(string.Format(Errors.ERROR_EVENTOPTION_CREATE_UNEXPECTED, evOption.EventOptionName), _logger);
                    }
                }
                catch (Exception ex) {
                    this.AddModelError(ex.Message, _logger, ex);
                }
            }
            else {
                this.AddModelError(ModelState.Values.SelectMany(v => v.Errors).ToList(), _logger);
            }

            return View("Modify", evOption);
        }

        //
        // GET: /EventOption/Edit/5
        public ActionResult Edit(int id) {
            return Modify(id);
        }

        //
        // POST: /EventOption/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EventOption evOption) {
            if (ModelState.IsValid) {
                try {
                    evOption.EmailOption = true;
                    fsWebService.UpdateEventOption(evOption);
                    return RedirectToAction("Index");
                }
                catch (Exception ex) {
                    this.AddModelError(ex.Message, _logger, ex);
                }
            }
            else {
                this.AddModelError(ModelState.Values.SelectMany(v => v.Errors).ToList(), _logger);
            }

            return View("Modify", evOption);
        }

        //
        // GET: /EventOption/Delete/5
        public ActionResult Delete(int id = 0) {
            EventOption evOption = fsWebService.GetEventOption(id);
            return PartialView("_DeleteConfirmation", evOption);
        }

        //
        // POST: /EventOption/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) {
            try {
                EventOption evOption = fsWebService.DeleteEventOption(id);
                if (evOption == null)
                    this.AddModelError(string.Format(Errors.ERROR_EVENTOPTION_DELETE, id), _logger);
            }
            catch (Exception ex) {
                this.AddModelError(ex.Message, _logger, ex);
            }

            return RedirectToAction("Index");
        }
        
        #region Helpers
        private ActionResult Modify(int id) {
            try {
                EventOption evOption = (id != 0) ? fsWebService.GetEventOption(id) : null;
                return ((evOption != null) ? View("Modify", evOption) : View("Modify"));
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                return HttpNotFound(ex.Message);
            }
        }
        #endregion // Helpers
    }
}
