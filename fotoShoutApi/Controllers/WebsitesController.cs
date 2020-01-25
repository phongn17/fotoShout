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
using FotoShoutData.Models;

namespace FotoShoutApi.Controllers
{
    public class WebsitesController : FotoShoutController
    {
        private readonly FotoShoutDbContext db = new FotoShoutDbContext();

        // GET api/Websites
        public IEnumerable<Website> GetWebsites() {
            return db.Websites.Where(s => s.User.Id == CurrentUser.Id).OrderBy(s => s.WebsiteName).AsEnumerable();
        }

        // GET api/Websites/5
        public Website GetWebsite(int id) {
            Website website = db.Websites.Where(s => s.WebsiteId == id && s.User.Id == CurrentUser.Id).SingleOrDefault();
            if (website == null) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return website;
        }

        // PUT api/Websites/5
        public HttpResponseMessage PutWebsite(int id, Website website)
        {
            if (ModelState.IsValid)
            {
                if (id != website.WebsiteId)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The event website identifier does not match the given id.");
                
                // Check for duplications
                Website temp = db.Websites.Where(s => s.WebsiteName == website.WebsiteName && s.WebsiteId != id && s.User.Id == CurrentUser.Id).SingleOrDefault();
                if (temp != null)
                    return Request.CreateErrorResponse(HttpStatusCode.Conflict, string.Format("{0} already exists.", website.WebsiteName));
                
                db.Entry(website).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // POST api/Websites
        public HttpResponseMessage PostWebsite(Website website)
        {
            if (ModelState.IsValid)
            {
                // Check for duplications
                Website temp = db.Websites.Where(s => s.WebsiteName == website.WebsiteName && s.User.Id == CurrentUser.Id).SingleOrDefault();
                if (temp != null)
                    return Request.CreateResponse(HttpStatusCode.Conflict, string.Format("{0} already exists.", website.WebsiteName));

                website.User = db.Users.Where(u => u.Id == CurrentUser.Id).SingleOrDefault();
                db.Websites.Add(website);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, website);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = website.WebsiteId }));
                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // DELETE api/Websites/5
        public HttpResponseMessage DeleteWebsite(int id) {
            Website website = db.Websites.Where(s => s.WebsiteId == id && s.User.Id == CurrentUser.Id).SingleOrDefault();
            if (website == null) {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Websites.Remove(website);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, website);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}