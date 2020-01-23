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
    public class EmailTemplateController : AuthenticatedController {
        static ILog _logger = LogManager.GetLogger(typeof(EmailTemplateController));
        
        //
        // GET: /EmailTemplate/
        public ActionResult Index() {
            return View(fsWebService.GetEmailTemplates());
        }

        //
        // GET: /EmailTemplate/Create
        public ActionResult Create() {
            return Modify(0);
        }

        //
        // POST: /EmailTemplate/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(EmailTemplate emailTemplate) {
            if (ModelState.IsValid) {
                try {
                    if (fsWebService.CreateEmailTemplate(emailTemplate) != null)
                        return RedirectToAction("Index");
                    else {
                        this.AddModelError(string.Format(Errors.ERROR_EMAILTEMPLATE_CREATE_UNEXPECTED, emailTemplate.EmailTemplateName), _logger);
                    }
                }
                catch (Exception ex) {
                    this.AddModelError(ex.Message.EndsWith("(409) Conflict.") ? string.Format("The \"{0}\" email template is already existed.", emailTemplate.EmailTemplateName) : ex.Message, _logger, ex);
                }
            }
            else {
                this.AddModelError(ModelState.Values.SelectMany(v => v.Errors).ToList(), _logger);
            }

            return View("Modify", emailTemplate);
        }

        //
        // GET: /EmailTemplate/Edit
        public ActionResult Edit(int id) {
            return Modify(id);
        }

        //
        // POST: /EmailTemplate/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(EmailTemplate emailTemplate) {
            if (ModelState.IsValid) {
                try {
                    fsWebService.UpdateEmailTemplate(emailTemplate);
                    return RedirectToAction("Index");
                }
                catch (Exception ex) {
                    this.AddModelError(ex.Message, _logger, ex);
                }
            }
            else {
                this.AddModelError(ModelState.Values.SelectMany(v => v.Errors).ToList(), _logger);
            }

            return View("Modify", emailTemplate);
        }

        //
        // GET: /EmailTemplate/Delete/5
        public ActionResult Delete(int id = 0) {
            EmailTemplate emailTemplate = fsWebService.GetEmailTemplate(id);
            return PartialView("_DeleteConfirmation", emailTemplate);
        }

        //
        // POST: /EmailTemplate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) {
            try {
                EmailTemplate emailTemplate = fsWebService.DeleteEmailTemplate(id);
                if (emailTemplate == null)
                    this.AddModelError(string.Format(Errors.ERROR_EMAILTEMPLATE_DELETE, id), _logger);
            }
            catch (Exception ex) {
                this.AddModelError(ex.Message, _logger, ex);
            }

            return RedirectToAction("Index");
        }

        //
        // GET: /EmailTemplate/LoadTemplate
        public ActionResult LoadTemplate() {
            return PartialView("_EmailDefaultTemplate");
        }
        
        #region Helpers
        private ActionResult Modify(int id) {
            try {
                EmailTemplate emailTemplate = (id != 0) ? fsWebService.GetEmailTemplate(id) : null;
                return  ((emailTemplate != null) ? View("Modify", emailTemplate) : View("Modify"));
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                return HttpNotFound(ex.Message);
            }
        }
        #endregion // Helpers
    }
}
