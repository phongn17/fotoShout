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
    public class SponsorsController : FotoShoutController
    {
        private FotoShoutDbContext db = new FotoShoutDbContext();

        // GET api/Sponsors
        public IEnumerable<Sponsor> GetSponsors() {
            return db.Sponsors.Where(s => s.User.Id == CurrentUser.Id).OrderBy(s => s.SponsorName).AsEnumerable();
        }

        // GET api/Sponsors/5
        public Sponsor GetSponsor(int id) {
            Sponsor sponsor = db.Sponsors.Where(s => s.SponsorId == id && s.User.Id == CurrentUser.Id).SingleOrDefault();
            if (sponsor == null) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return sponsor;
        }

        // PUT api/Sponsors/5
        public HttpResponseMessage PutSponsor(int id, Sponsor sponsor)
        {
            if (ModelState.IsValid)
            {
                if (id != sponsor.SponsorId)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The event sponsor identifier does not match the given id.");
                
                // Check for duplications
                Sponsor temp = db.Sponsors.Where(s => s.SponsorName == sponsor.SponsorName && s.SponsorId != id && s.User.Id == CurrentUser.Id).SingleOrDefault();
                if (temp != null)
                    return Request.CreateErrorResponse(HttpStatusCode.Conflict, string.Format("{0} already exists.", sponsor.SponsorName));
                
                db.Entry(sponsor).State = EntityState.Modified;

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

        // POST api/Sponsors
        public HttpResponseMessage PostSponsor(Sponsor sponsor)
        {
            if (ModelState.IsValid)
            {
                // Check for duplications
                Sponsor temp = db.Sponsors.Where(s => s.SponsorName == sponsor.SponsorName && s.User.Id == CurrentUser.Id).SingleOrDefault();
                if (temp != null)
                    return Request.CreateResponse(HttpStatusCode.Conflict, string.Format("{0} already exists.", sponsor.SponsorName));

                sponsor.User = db.Users.Where(u => u.Id == CurrentUser.Id).SingleOrDefault();
                db.Sponsors.Add(sponsor);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, sponsor);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = sponsor.SponsorId }));
                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // DELETE api/Sponsors/5
        public HttpResponseMessage DeleteSponsor(int id) {
            Sponsor sponsor = db.Sponsors.Where(s => s.SponsorId == id && s.User.Id == CurrentUser.Id).SingleOrDefault();
            if (sponsor == null) {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Sponsors.Remove(sponsor);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, sponsor);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}