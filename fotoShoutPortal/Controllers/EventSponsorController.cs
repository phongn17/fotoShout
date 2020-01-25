using FotoShoutData.Models;
using FotoShoutUtils;
using FotoShoutUtils.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutPortal.Controllers {
    [Authorize]
    public class EventSponsorController : AuthenticatedController {
        static ILog _logger = LogManager.GetLogger(typeof(EventSponsorController));
        //
        // GET: /EventSponsor/
        public ActionResult Index() {
            return View(fsWebService.GetEventSponsors());
        }

        //
        // GET: /EventSponsor/Create
        public ActionResult Create() {
            return Modify(0);
        }

        //
        // POST: /EventSponsor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SponsorTDO tdo) {
            if (ModelState.IsValid) {
                try {
                    var sponsor = this.SponsorTDO2Sponsor(tdo);
                    if (fsWebService.CreateEventSponsor(sponsor) != null)
                        return RedirectToAction("Index");
                    else {
                        this.AddModelError(string.Format(Errors.ERROR_EVENTSPONSOR_CREATE_UNEXPECTED, sponsor.SponsorName), _logger);
                    }
                }
                catch (Exception ex) {
                    this.AddModelError(ex.Message, _logger, ex);
                }
            }
            else {
                this.AddModelError(ModelState.Values.SelectMany(v => v.Errors).ToList(), _logger);
            }

            return View("Modify", tdo);
        }

        //
        // GET: /EventSponsor/Edit/5
        public ActionResult Edit(int id) {
            return Modify(id);
        }

        //
        // POST: /EventSponsor/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SponsorTDO tdo) {
            if (ModelState.IsValid) {
                try {
                    var sponsor = this.SponsorTDO2Sponsor(tdo);
                    fsWebService.UpdateEventSponsor(sponsor);
                    return RedirectToAction("Index");
                }
                catch (Exception ex) {
                    this.AddModelError(ex.Message, _logger, ex);
                }
            }
            else {
                this.AddModelError(ModelState.Values.SelectMany(v => v.Errors).ToList(), _logger);
            }

            return View("Modify", tdo);
        }

        //
        // GET: /EventSponsor/Delete/5
        public ActionResult Delete(int id = 0) {
            Sponsor sponsor = fsWebService.GetEventSponsor(id);
            return PartialView("_DeleteConfirmation", sponsor);
        }

        //
        // POST: /EventSponsor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) {
            try {
                Sponsor sponsor = fsWebService.DeleteEventSponsor(id);
                if (sponsor == null)
                    this.AddModelError(string.Format(Errors.ERROR_EVENTSPONSOR_DELETE, id), _logger);
            }
            catch (Exception ex) {
                this.AddModelError(ex.Message, _logger, ex);
            }

            return RedirectToAction("Index");
        }

        #region Helpers
        private ActionResult Modify(int id) {
            try {
                Sponsor sponsor = (id != 0) ? fsWebService.GetEventSponsor(id) : null;
                return ((sponsor != null) ? View("Modify", this.Sponsor2SponsorTDO(sponsor)) : View("Modify"));
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                return HttpNotFound(ex.Message);
            }
        }

        private SponsorTDO Sponsor2SponsorTDO(Sponsor sponsor) {
            SponsorTDO tdo = new SponsorTDO {
                SponsorId = sponsor.SponsorId,
                SponsorName = sponsor.SponsorName,
                SponsorLogo = sponsor.SponsorLogo,
            };

            return tdo;
        }
        
        private Sponsor SponsorTDO2Sponsor(SponsorTDO tdo) {
            Sponsor sponsor = new Sponsor {
                SponsorId = tdo.SponsorId,
                SponsorName = tdo.SponsorName,
                SponsorLogo = tdo.SponsorLogo,
            };

            return sponsor;
        }

        #endregion // Helpers
    }
}
