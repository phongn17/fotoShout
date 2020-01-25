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
    public class EventWebsiteController : AuthenticatedController {
        readonly static ILog _logger = LogManager.GetLogger(typeof(EventWebsiteController));
        //
        // GET: /EventWebsite/
        public ActionResult Index() {
            return View(fsWebService.GetEventWebsites());
        }

        //
        // GET: /EventWebsite/Create
        public ActionResult Create() {
            return Modify(0);
        }

        //
        // POST: /EventWebsite/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WebsiteTDO tdo) {
            ValidateWebsiteTDO(tdo);

            if (ModelState.IsValid) {
                try {
                    var website = this.WebsiteTDO2Website(tdo);
                    if (fsWebService.CreateEventWebsite(website) != null)
                        return RedirectToAction("Index");
                    else {
                        this.AddModelError(string.Format(Errors.ERROR_EVENTSPONSOR_CREATE_UNEXPECTED, website.WebsiteName), _logger);
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
        // GET: /EventWebsite/Edit/5
        public ActionResult Edit(int id) {
            return Modify(id);
        }

        //
        // POST: /EventWebsite/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WebsiteTDO tdo) {
            ValidateWebsiteTDO(tdo);

            if (ModelState.IsValid) {
                try {
                    var website = this.WebsiteTDO2Website(tdo);
                    fsWebService.UpdateEventWebsite(website);
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
        // GET: /EventWebsite/Delete/5
        public ActionResult Delete(int id = 0) {
            Website website = fsWebService.GetEventWebsite(id);
            return PartialView("_DeleteConfirmation", website);
        }

        //
        // POST: /EventWebsite/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) {
            try {
                Website website = fsWebService.DeleteEventWebsite(id);
                if (website == null)
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
                Website website = (id != 0) ? fsWebService.GetEventWebsite(id) : null;
                return ((website != null) ? View("Modify", this.Website2WebsiteTDO(website)) : View("Modify"));
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                return HttpNotFound(ex.Message);
            }
        }

        private WebsiteTDO Website2WebsiteTDO(Website website) {
            WebsiteTDO tdo = new WebsiteTDO {
                WebsiteId = website.WebsiteId,
                WebsiteName = website.WebsiteName,
                HeaderUrl = website.HeaderUrl,
                FooterUrl = website.FooterUrl,
                TopInfoBlockUrl = website.TopInfoBlockUrl,
                BottomInfoBlockUrl = website.BottomInfoBlockUrl,
                HeaderImage = website.HeaderImage,
                FooterImage = website.FooterImage,
                TopInfoBlockImage = website.TopInfoBlockImage,
                BottomInfoBlockImage = website.BottomInfoBlockImage
            };

            return tdo;
        }
        
        private Website WebsiteTDO2Website(WebsiteTDO tdo) {
            Website website = new Website {
                WebsiteId = tdo.WebsiteId,
                WebsiteName = tdo.WebsiteName,
                HeaderUrl = tdo.HeaderUrl,
                FooterUrl = tdo.FooterUrl,
                TopInfoBlockUrl = tdo.TopInfoBlockUrl,
                BottomInfoBlockUrl = tdo.BottomInfoBlockUrl,
                HeaderImage = tdo.HeaderImage,
                FooterImage = tdo.FooterImage,
                TopInfoBlockImage = tdo.TopInfoBlockImage,
                BottomInfoBlockImage = tdo.BottomInfoBlockImage
            };

            var uploadDir = "~/uploads";
            var uploadPath = Server.MapPath(uploadDir);
            var imagePath = "";
            DirectoryInfo dirInfo = new DirectoryInfo(uploadPath);
            if (!dirInfo.Exists) {
                Directory.CreateDirectory(uploadPath);
            }

            if (tdo.HeaderFile != null) {
                var websiteHeaderImage = Path.Combine(uploadDir, tdo.HeaderFile.FileName).Replace('\\', '/');
                if (websiteHeaderImage != website.HeaderImage) {
                    website.HeaderImage = websiteHeaderImage;
                    imagePath = Path.Combine(uploadPath, tdo.HeaderFile.FileName);
                    tdo.HeaderFile.SaveAs(imagePath);
                }
            }

            if (tdo.FooterFile != null) {
                var websiteFooterImage = Path.Combine(uploadDir, tdo.FooterFile.FileName).Replace('\\', '/');
                if (websiteFooterImage != website.FooterImage) {
                    website.FooterImage = websiteFooterImage;
                    imagePath = Path.Combine(uploadPath, tdo.FooterFile.FileName);
                    tdo.FooterFile.SaveAs(imagePath);
                }
            }

            if (tdo.TopInfoBlockFile != null) {
                var websiteTopInfoBlockImage = Path.Combine(uploadDir, tdo.TopInfoBlockFile.FileName).Replace('\\', '/');
                if (websiteTopInfoBlockImage != website.TopInfoBlockImage) {
                    website.TopInfoBlockImage = websiteTopInfoBlockImage;
                    imagePath = Path.Combine(uploadPath, tdo.TopInfoBlockFile.FileName);
                    tdo.TopInfoBlockFile.SaveAs(imagePath);
                }
            }

            if (tdo.BottomInfoBlockFile != null) {
                var websiteBottomInfoBlockImage = Path.Combine(uploadDir, tdo.BottomInfoBlockFile.FileName).Replace('\\', '/');
                if (websiteBottomInfoBlockImage != website.BottomInfoBlockImage) {
                    website.BottomInfoBlockImage = websiteBottomInfoBlockImage;
                    imagePath = Path.Combine(uploadPath, tdo.BottomInfoBlockFile.FileName);
                    tdo.BottomInfoBlockFile.SaveAs(imagePath);
                }
            }

            return website;
        }

        private void ValidateWebsiteTDO(WebsiteTDO tdo) {
            var imageTypes = ImageUtils.GetImageTypes();
            if (tdo.HeaderFile != null) {
                if (!imageTypes.Contains(tdo.HeaderFile.ContentType)) {
                    ModelState.AddModelError("HeaderFile", "Please choose either a JPG or PNG image for the 'Header Image' field.");
                    return;
                }
            }

            if (tdo.FooterFile != null) {
                if (!imageTypes.Contains(tdo.FooterFile.ContentType)) {
                    ModelState.AddModelError("FooterFile", "Please choose either a JPG or PNG image for the 'Footer Image' field.");
                    return;
                }
            }

            if (tdo.TopInfoBlockFile != null) {
                if (!imageTypes.Contains(tdo.TopInfoBlockFile.ContentType)) {
                    ModelState.AddModelError("TopInfoBlockFile", "Please choose either a JPG or PNG image for the 'Top Block Image' field.");
                    return;
                }
            }

            if (tdo.BottomInfoBlockFile != null) {
                if (!imageTypes.Contains(tdo.BottomInfoBlockFile.ContentType)) {
                    ModelState.AddModelError("BottomInfoBlockFile", "Please choose either a JPG or PNG image for the 'Bottom Block Image' field.");
                    return;
                }
            }
        }

        #endregion // Helpers
    }
}
