using FotoShoutApi.Models;
using FotoShoutData.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FotoShoutApi.Controllers {
    public class EmailTemplatesController : FotoShoutController {
        static ILog _logger = LogManager.GetLogger(typeof(EmailTemplatesController));
        
        private FotoShoutDbContext db = new FotoShoutDbContext();

        // GET api/EmailTemplates
        public IEnumerable<EmailTemplate> GetEmailTemplates() {
            return db.EmailTemplates.Where(e => e.User.Id == CurrentUser.Id).OrderBy(e => e.EmailTemplateName).AsEnumerable();
        }
        
        // GET api/EmailTemplates/5
        public EmailTemplate GetEmailTemplate(int id) {
            EmailTemplate emailTemplate = db.EmailTemplates.Where(e => e.EmailTemplateId == id && e.User.Id == CurrentUser.Id).SingleOrDefault();
            if (emailTemplate == null) {
                string msg = string.Format("The {0} email template does not exist.", id);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound) {
                    Content = new StringContent(msg),
                    ReasonPhrase = msg
                });
            }

            return emailTemplate;
        }

        // PUT api/EmailTemplates/5
        public HttpResponseMessage PutEmailTemplate(int id, EmailTemplate emailTemplate) {
            try {
                if (ModelState.IsValid) {
                    if (id != emailTemplate.EmailTemplateId)
                        this.GenerateException(HttpStatusCode.BadRequest, "The email template identifier does not match the given id.", _logger);

                    var temp = db.EmailTemplates.Where(e => e.EmailTemplateName == emailTemplate.EmailTemplateName && e.EmailTemplateId != id).SingleOrDefault();
                    if (temp != null)
                        this.GenerateException(HttpStatusCode.Conflict, string.Format("{0} already exists.", emailTemplate.EmailTemplateName), _logger);

                    db.Entry(emailTemplate).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState.Values.SelectMany(v => v.Errors));
                }
            }
            catch (HttpResponseException ex) {
                throw ex;
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.InternalServerError, ex.Message);
            }
            
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/EmailTemplates
        public HttpResponseMessage PostEmailTemplate(EmailTemplate emailTemplate) {
            try {
                if (ModelState.IsValid) {
                    var temp = db.EmailTemplates.Where(e => e.EmailTemplateName == emailTemplate.EmailTemplateName && e.User.Id == CurrentUser.Id).SingleOrDefault();
                    if (temp != null)
                        this.GenerateException(HttpStatusCode.Conflict, string.Format("{0} already exists.", emailTemplate.EmailTemplateName), _logger);

                    emailTemplate.User = db.Users.Where(u => u.Id == CurrentUser.Id).SingleOrDefault();
                    db.EmailTemplates.Add(emailTemplate);
                    db.SaveChanges();
                }
                else {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState.Values.SelectMany(v => v.Errors));
                }
            }
            catch (HttpResponseException ex) {
                throw ex;
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.InternalServerError, ex.Message);
            }
            
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, emailTemplate);
            response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = emailTemplate.EmailTemplateId }));
            return response;
        }

        // DELETE api/EmailTemplates/5
        public HttpResponseMessage DeleteEmailTemplate(int id) {
            EmailTemplate emailTemplate = null;
            try {
                emailTemplate = db.EmailTemplates.Where(e => e.EmailTemplateId == id && e.User.Id == CurrentUser.Id).SingleOrDefault();
                if (emailTemplate == null) {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                db.EmailTemplates.Remove(emailTemplate);
                db.SaveChanges();
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, emailTemplate);
        }
        
        protected override void Dispose(bool disposing) {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
