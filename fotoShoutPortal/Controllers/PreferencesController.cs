using FotoShoutData.Models;
using FotoShoutUtils;
using FotoShoutUtils.Service;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutPortal.Controllers {
    [Authorize]
    public class PreferencesController : AuthenticatedController {
        static ILog _logger = LogManager.GetLogger(typeof(PreferencesController));
        
        const string PREFER_PUBLISHCONFIGURATION = "PublishConfiguration";
        const string PREFER_EVENTOPTIONS = "EventOption";
        const string PREFER_EVENTSPONSORS = "EventSponsor";
        const string PREFER_EMAILSERVERCONFIGURATION = "EmailServerConfiguration";
        const string PREFER_EMAILTEMPLATES = "EmailTemplate";
        //
        // GET: /Preferences/

        public ActionResult Index(string preferName = "", object routeValues = null) {
            switch (preferName) {
                case PreferencesController.PREFER_EVENTOPTIONS:
                    return RedirectToAction("Index", "EventOption");

                case PreferencesController.PREFER_EVENTSPONSORS:
                    return RedirectToAction("Index", "EventSponsor");

                case PreferencesController.PREFER_EMAILTEMPLATES:
                    return RedirectToAction("Index", "EmailTemplate");

                case PreferencesController.PREFER_EMAILSERVERCONFIGURATION:
                case PreferencesController.PREFER_PUBLISHCONFIGURATION:
                    return RedirectToAction(preferName, routeValues);

                default:
                    return RedirectToAction(PreferencesController.PREFER_PUBLISHCONFIGURATION, routeValues);

            }
        }

        public ActionResult EmailServerConfiguration() {
            try {
                EmailServerAccount model = fsWebService.GetEmailServerConfiguration();
                return View(model);
            }
            catch (HttpClientServiceException ex) {
                if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                    this.AddModelError(ex.Message, _logger, ex);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Debug(_logger, ex.ToString());
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmailServerConfiguration(EmailServerAccount model) {
            try {
                if (ModelState.IsValid) {
                    EmailServerAccount temp = fsWebService.EmailServerConfiguration(model);
                    if (temp == null && model.EmailServerAccountId == 0)
                        this.AddModelError(Errors.ERROR_EMAILSERVERCONFIG, _logger);
                    else {
                        if (temp != null)
                            model = temp;
                        this.AddMessage(Constants.INFO_EMAILSERVERCONFIG_SUCCESS);
                    }
                }
                else {
                    this.AddModelError(ModelState.Values.SelectMany(v => v.Errors).ToList(), _logger);
                }
            }
            catch (Exception ex) {
                this.AddModelError(ex.Message, _logger, ex);
            }

            return View(model);
        }

        public ActionResult PublishConfiguration() {
            try {
                PublishAccount model = fsWebService.GetPublishConfiguration();
                return View(model);
            }
            catch (HttpClientServiceException ex) {
                if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                    this.AddModelError(ex.Message, _logger, ex);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Debug(_logger, ex.ToString());
            }
            return View(new PublishAccount {
                ApiKey = Guid.Parse(AppConfigs.C9ApiKey)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PublishConfiguration(PublishAccount model) {
            try {
                if (ModelState.IsValid) {
                    GeneratePublishService(model);
                    PublishAccount temp = fsWebService.PublishConfiguration(model);
                    if (temp == null && model.Id == 0)
                        this.AddModelError(Errors.ERROR_PUBLISHCONFIG, _logger);
                    else {
                        if (temp != null)
                            model = temp;
                        this.AddMessage(Constants.INFO_PUBLISHCONFIG_SUCCESS);
                    }
                }
                else {
                    this.AddModelError(ModelState.Values.SelectMany(v => v.Errors).ToList(), _logger);
                }
            }
            catch (Exception ex) {
                this.AddModelError(ex.Message, _logger, ex);
            }

            return View(model);
        }
    }
}
