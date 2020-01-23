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
using FotoShoutApi.Services;
using FotoShoutData.Models;

namespace FotoShoutApi.Controllers
{
    public class EventGuestsController : FotoShoutController {
        private FotoShoutDbContext db = new FotoShoutDbContext();

        // GET api/Guests/1
        public IEnumerable<GuestTDO> GetGuests(int id) {
            return PhotoAnnotationService.GetGuestTDOs(id, db);
        }

        // id to Guid
        // GET api/Guests/1?guestId=<guest id>
        public GuestTDO GetGuest(int id, Guid guestId) {
            return PhotoAnnotationService.GetGuestTDOById(id, guestId, db);
        }

        // GET api/Guests/1?email=<email>
        public GuestTDO GetGuestByEmail(int id, string email) {
            return PhotoAnnotationService.GetGuestTDOByEmail(id, email, db);
        }
        
        protected override void Dispose(bool disposing) {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}