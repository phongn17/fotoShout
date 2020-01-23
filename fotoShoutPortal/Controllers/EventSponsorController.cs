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
            ValidateSponsorTDO(tdo);

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
            ValidateSponsorTDO(tdo);

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
                SponsorHeaderUrl = sponsor.SponsorHeaderUrl,
                SponsorFooterUrl = sponsor.SponsorFooterUrl,
                SponsorTopInfoBlockUrl = sponsor.SponsorTopInfoBlockUrl,
                SponsorBottomInfoBlockUrl = sponsor.SponsorBottomInfoBlockUrl,
                SponsorHeaderImage = sponsor.SponsorHeaderImage,
                SponsorFooterImage = sponsor.SponsorFooterImage,
                SponsorTopInfoBlockImage = sponsor.SponsorTopInfoBlockImage,
                SponsorBottomInfoBlockImage = sponsor.SponsorBottomInfoBlockImage
            };

            return tdo;
        }
        
        private Sponsor SponsorTDO2Sponsor(SponsorTDO tdo) {
            Sponsor sponsor = new Sponsor {
                SponsorId = tdo.SponsorId,
                SponsorName = tdo.SponsorName,
                SponsorLogo = tdo.SponsorLogo,
                SponsorHeaderUrl = tdo.SponsorHeaderUrl,
                SponsorFooterUrl = tdo.SponsorFooterUrl,
                SponsorTopInfoBlockUrl = tdo.SponsorTopInfoBlockUrl,
                SponsorBottomInfoBlockUrl = tdo.SponsorBottomInfoBlockUrl,
                SponsorHeaderImage = tdo.SponsorHeaderImage,
                SponsorFooterImage = tdo.SponsorFooterImage,
                SponsorTopInfoBlockImage = tdo.SponsorTopInfoBlockImage,
                SponsorBottomInfoBlockImage = tdo.SponsorBottomInfoBlockImage
            };

            var uploadDir = "~/uploads";
            var uploadPath = Server.MapPath(uploadDir);
            var imagePath = "";
            DirectoryInfo dirInfo = new DirectoryInfo(uploadPath);
            if (!dirInfo.Exists) {
                Directory.CreateDirectory(uploadPath);
            }

            if (tdo.SponsorHeaderFile != null) {
                var sponsorHeaderImage = Path.Combine(uploadDir, tdo.SponsorHeaderFile.FileName).Replace('\\', '/');
                if (sponsorHeaderImage != sponsor.SponsorHeaderImage) {
                    sponsor.SponsorHeaderImage = sponsorHeaderImage;
                    imagePath = Path.Combine(uploadPath, tdo.SponsorHeaderFile.FileName);
                    tdo.SponsorHeaderFile.SaveAs(imagePath);
                }
            }

            if (tdo.SponsorFooterFile != null) {
                var sponsorFooterImage = Path.Combine(uploadDir, tdo.SponsorFooterFile.FileName).Replace('\\', '/');
                if (sponsorFooterImage != sponsor.SponsorFooterImage) {
                    sponsor.SponsorFooterImage = sponsorFooterImage;
                    imagePath = Path.Combine(uploadPath, tdo.SponsorFooterFile.FileName);
                    tdo.SponsorFooterFile.SaveAs(imagePath);
                }
            }

            if (tdo.SponsorTopInfoBlockFile != null) {
                var sponsorTopInfoBlockImage = Path.Combine(uploadDir, tdo.SponsorTopInfoBlockFile.FileName).Replace('\\', '/');
                if (sponsorTopInfoBlockImage != sponsor.SponsorTopInfoBlockImage) {
                    sponsor.SponsorTopInfoBlockImage = sponsorTopInfoBlockImage;
                    imagePath = Path.Combine(uploadPath, tdo.SponsorTopInfoBlockFile.FileName);
                    tdo.SponsorTopInfoBlockFile.SaveAs(imagePath);
                }
            }

            if (tdo.SponsorBottomInfoBlockFile != null) {
                var sponsorBottomInfoBlockImage = Path.Combine(uploadDir, tdo.SponsorBottomInfoBlockFile.FileName).Replace('\\', '/');
                if (sponsorBottomInfoBlockImage != sponsor.SponsorBottomInfoBlockImage) {
                    sponsor.SponsorBottomInfoBlockImage = sponsorBottomInfoBlockImage;
                    imagePath = Path.Combine(uploadPath, tdo.SponsorBottomInfoBlockFile.FileName);
                    tdo.SponsorBottomInfoBlockFile.SaveAs(imagePath);
                }
            }

            return sponsor;
        }

        private void ValidateSponsorTDO(SponsorTDO tdo) {
            var imageTypes = ImageUtils.GetImageTypes();
            if (tdo.SponsorHeaderFile != null) {
                if (!imageTypes.Contains(tdo.SponsorHeaderFile.ContentType)) {
                    ModelState.AddModelError("SponsorHeaderFile", "Please choose either a JPG or PNG image for the 'Header Image' field.");
                    return;
                }
            }

            if (tdo.SponsorFooterFile != null) {
                if (!imageTypes.Contains(tdo.SponsorFooterFile.ContentType)) {
                    ModelState.AddModelError("SponsorFooterFile", "Please choose either a JPG or PNG image for the 'Footer Image' field.");
                    return;
                }
            }

            if (tdo.SponsorTopInfoBlockFile != null) {
                if (!imageTypes.Contains(tdo.SponsorTopInfoBlockFile.ContentType)) {
                    ModelState.AddModelError("SponsorTopInfoBlockFile", "Please choose either a JPG or PNG image for the 'Top Block Image' field.");
                    return;
                }
            }

            if (tdo.SponsorBottomInfoBlockFile != null) {
                if (!imageTypes.Contains(tdo.SponsorBottomInfoBlockFile.ContentType)) {
                    ModelState.AddModelError("SponsorBottomInfoBlockFile", "Please choose either a JPG or PNG image for the 'Bottom Block Image' field.");
                    return;
                }
            }
        }

        #endregion // Helpers
    }
}
