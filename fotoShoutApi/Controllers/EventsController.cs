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
using FotoShoutApi.Utils;
using System.IO;
using FotoShoutApi.Security;
using FotoShoutApi.Services;
using System.Web.Hosting;
using FotoShoutApi.Utils.IO;
using log4net;
using FotoShoutData.Models;
using FotoShoutUtils.Utils;
using FotoShoutUtils.Utils.IO;
using System.Data.Entity.Validation;
using System.Text;

namespace FotoShoutApi.Controllers
{
    public class EventsController : FotoShoutController
    {
        static ILog _logger = LogManager.GetLogger(typeof(EventsController));

        internal static bool ClearEventRelationships(Event ev, FotoShoutDbContext db) {
            try {
                // Remove all photos that are associated to the event
                foreach (Photo p in ev.Photos)
                    db.Photos.Remove(p);
                
                // Clear all associations to sponsors
                ev.Sponsors.Clear();
                
                return true;
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = ex.Message, Content = new StringContent(ex.Message) });
            }
        }

        internal static Event GetEvent(int id, int userId, HttpRequestMessage request, FotoShoutDbContext db, bool isIncludingPhotos = true, bool isIncludingGuests = false) {
            Event ev = null;
            try {
                if (isIncludingPhotos && isIncludingGuests) {
                    ev = db.Events.Where(e => e.EventId == id && e.User.Id == userId).Include(e => e.Photos).Include(e => e.Guests).Include(e => e.EventOption).Include(e => e.EmailTemplate).SingleOrDefault();
                }
                else if (isIncludingPhotos) {
                    ev = db.Events.Where(e => e.EventId == id && e.User.Id == userId).Include(e => e.Photos).Include(e => e.EventOption).Include(e => e.EmailTemplate).SingleOrDefault();
                }
                else {
                    ev = db.Events.Where(e => e.EventId == id && e.User.Id == userId).Include(e => e.EventOption).Include(e => e.EmailTemplate).SingleOrDefault();
                }
            }
            catch (Exception ex) {
                throw new HttpResponseException(request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message));
            }

            if (ev == null)
                throw new HttpResponseException(request.CreateResponse(HttpStatusCode.NotFound, string.Format(Errors.ERROR_EVENT_NOTFOUND, id)));

            return ev;
        }

        private FotoShoutDbContext db = new FotoShoutDbContext();

        // GET api/Events
        public IEnumerable<EventTDO> GetEvents(string eventType = "")
        {
            EventStatus evType = EventPhotosService.GetEventStatus(eventType);
            HashSet<EventTDO> tdos = new HashSet<EventTDO>();
            try {
                IEnumerable<Event> events = (evType != EventStatus.Undefined) ? 
                    db.Events.Where(ev => ev.User.Id == CurrentUser.Id && ev.EventStatus == (byte)evType).Include(e => e.EventOption).OrderBy(ev => ev.EventName).ToList() :
                    db.Events.Where(ev => ev.User.Id == CurrentUser.Id).Include(e => e.EventOption).OrderBy(ev => ev.EventName).ToList();
                if (events.Any()) {
                    foreach (Event ev in events) {
                        string error = DirectoryUtils.GenerateDirectories(ev.EventFolder);
                        if (!string.IsNullOrEmpty(error)) {
                            this.GenerateException(HttpStatusCode.InternalServerError, error, _logger);
                        }
                        FotoShoutApi.Services.IO.DirectoryService.GenerateVirtualPath(ev.EventFolder);

                        if (evType != EventStatus.Undefined || EventPhotosService.IsEventType(ev, eventType)) {
                            EventTDO tdo = new EventTDO {
                                EventId = ev.EventId,
                                EventName = ev.EventName,
                                EventDescription = ev.EventDescription,
                                EventDate = ev.EventDate,
                                EventLocation = ev.EventLocation,
                                EventFolder = ev.EventFolder,
                                EventVirtualPath = ev.EventVirtualPath,
                                Created = ev.Created,
                                CreatedBy = ev.CreatedBy,
                                EventOptionId = ev.EventOption.EventOptionId,
                                EventStatus = ev.EventStatus,
                                PublishAlbum = ev.PublishAlbum
                            };

                            try {
                                string relativeThumbnail = EventPhotosService.GetThumbnailUrl(tdo.EventId, tdo.EventFolder, tdo.EventVirtualPath, db);
                                tdo.Thumbnail = string.IsNullOrEmpty(relativeThumbnail) ? "" : AppConfigs.VirtualRoot + relativeThumbnail;
                            }
                            catch (ObjectNotFoundException) {
                                tdo.Thumbnail = "";
                            }
                            tdos.Add(tdo);
                        }
                    }
                }
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.InternalServerError, ex.Message);
            }

            return tdos;
        }

        // GET api/Events/1
        public EventTDO GetEvent(int id) {
            if (id == 0)
                return GenerateTDO();

            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            if (ev == null) {
                string msg = string.Format("The {0} event not found.", id.ToString());
                FotoShoutUtils.Log.LogManager.Error(_logger, msg);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = msg, Content = new StringContent(msg) });
            }

            return GenerateTDO(ev);
        }

        /// <summary>
        /// Get an event by name
        /// </summary>
        // GET api/Events?name=<Event Name>
        public EventTDO GetEventByName(string name) {
            Event ev = db.Events.Where(e => e.EventName.Contains(name, StringComparison.InvariantCultureIgnoreCase) && 
                                            e.User.Id == CurrentUser.Id).SingleOrDefault();

            if (ev == null) {
                string msg = string.Format("The {0} event not found.", name);
                FotoShoutUtils.Log.LogManager.Error(_logger, msg);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound) { ReasonPhrase = msg, Content = new StringContent(msg) });
            }

            return GenerateTDO(ev);
        }

        // PUT api/Events/1
        public HttpResponseMessage PutEvent(int id, EventTDO tdo)
        {
            if (ModelState.IsValid) {
                if (id != tdo.EventId)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The updated event's identifier does not match the given id.");

                // Check for duplicated names
                Event ev = db.Events.Where(e => e.EventName.Equals(tdo.EventName, StringComparison.InvariantCultureIgnoreCase) && 
                                                e.User.Id == CurrentUser.Id && e.EventId != id).SingleOrDefault();
                if (ev != null)
                    this.GenerateException(HttpStatusCode.InternalServerError, string.Format("The {0} event already exists.", tdo.EventName), _logger);
                
                ev = db.Events.Where(e => e.User.Id == CurrentUser.Id && e.EventId == id).SingleOrDefault();
                if (ev == null)
                    this.GenerateException(HttpStatusCode.InternalServerError, string.Format("Could not find an event with the {0} id.", id.ToString()), _logger);

                try {
                    string error = DirectoryUtils.GenerateDirectories(tdo.EventFolder);
                    if (!string.IsNullOrEmpty(error))
                        return Request.CreateResponse(HttpStatusCode.NotFound, error);
                    
                    ev.EventName = tdo.EventName;
                    ev.EventDescription = tdo.EventDescription;
                    ev.EventDate = tdo.EventDate;
                    ev.EventLocation = tdo.EventLocation;
                    ev.EventFolder = tdo.EventFolder;
                    ev.EventVirtualPath = FotoShoutApi.Services.IO.DirectoryService.GenerateVirtualPath(tdo.EventFolder);
                    ev.CreatedBy = CurrentUser.Id;
                    ev.Created = DateTime.Now;
                    ev.EventStatus = tdo.EventStatus;
                    ev.PublishAlbum = tdo.PublishAlbum;
                    ev.EventOption = (tdo.EventOptionId != 0) ? db.EventOptions.Where(eo => eo.EventOptionId == tdo.EventOptionId).SingleOrDefault() : null;
                    ev.EmailTemplate = (tdo.EmailTemplateId != 0) ? db.EmailTemplates.Where(e => e.EmailTemplateId == tdo.EmailTemplateId).SingleOrDefault() : null;
                    ICollection<Sponsor> sponsors = (tdo.SponsorIds != null) ? db.Sponsors.Where(s => tdo.SponsorIds.Contains(s.SponsorId)).ToList() : null;
                    if (sponsors != null) {
                        if (ev.Sponsors == null)
                            ev.Sponsors = sponsors;
                        else {
                            // Remove unselected sponsors from the list of the current sponsors
                            HashSet<Sponsor> removedSponsors = new HashSet<Sponsor>();
                            foreach (Sponsor s in ev.Sponsors) {
                                if (!tdo.SponsorIds.Contains(s.SponsorId))
                                    removedSponsors.Add(s);
                            }
                            foreach (Sponsor s in removedSponsors)
                                ev.Sponsors.Remove(s);

                            // Adding new sponsors to the list
                            foreach (Sponsor sponsor in sponsors) {
                                var temp = (from s in ev.Sponsors
                                            where s.SponsorId == sponsor.SponsorId
                                            select s).SingleOrDefault();
                                if (temp == null)
                                    ev.Sponsors.Add(sponsor);
                            }
                        }
                    }
                    else
                        ev.Sponsors.Clear();

                    ev.ChannelGroupId = tdo.ChannelGroupId;
                    ev.WebsiteId = tdo.WebsiteId;
                    
                    db.Entry(ev).State = EntityState.Modified;

                    db.SaveChanges();
                }
                catch (Exception ex) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                    this.GenerateException(HttpStatusCode.InternalServerError, ex.Message);
                }

                // Update the TDO object
                UpdateTDO(tdo, ev);
                
                return Request.CreateResponse(HttpStatusCode.OK, tdo);
            }
            else {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // POST api/Events
        public HttpResponseMessage PostEvent(EventTDO tdo)
        {
            if (ModelState.IsValid && tdo != null) {
                // Check for duplicated names
                var temp = db.Events.Where(e => e.EventName.Equals(tdo.EventName, StringComparison.InvariantCultureIgnoreCase) &&
                                                e.User.Id == CurrentUser.Id).SingleOrDefault();
                if (temp != null)
                    this.GenerateException(HttpStatusCode.InternalServerError, string.Format("The {0} event already exists.", tdo.EventName), _logger);

                Event ev = null;
                try {
                    string error = DirectoryUtils.GenerateDirectories(tdo.EventFolder);
                    if (!string.IsNullOrEmpty(error))
                        return Request.CreateResponse(HttpStatusCode.NotFound, error);

                    ev = new Event {
                        EventName = tdo.EventName,
                        EventDescription = tdo.EventDescription,
                        EventDate = tdo.EventDate,
                        EventLocation = tdo.EventLocation,
                        EventFolder = tdo.EventFolder,
                        EventVirtualPath = FotoShoutApi.Services.IO.DirectoryService.GenerateVirtualPath(tdo.EventFolder),
                        Created = DateTime.Now,
                        CreatedBy = CurrentUser.Id,
                        EventStatus = (byte)EventStatus.Open,
                        PublishAlbum = tdo.PublishAlbum,
                        User = db.Users.Where(u => u.Id == CurrentUser.Id).SingleOrDefault()
                    };

                    if (tdo.EventOptionId != 0) {
                        EventOption evo = db.EventOptions.Where(eo => eo.EventOptionId == tdo.EventOptionId).SingleOrDefault();
                        if (evo != null)
                            ev.EventOption = evo;
                    }

                    if (tdo.EmailTemplateId != 0) {
                        EmailTemplate emailTemplate = db.EmailTemplates.Where(e => e.EmailTemplateId == tdo.EmailTemplateId).SingleOrDefault();
                        if (emailTemplate != null)
                            ev.EmailTemplate = emailTemplate;
                    }

                    if (tdo.SponsorIds != null) {
                        IEnumerable<Sponsor> sponsors = db.Sponsors.Where(s => tdo.SponsorIds.Contains(s.SponsorId));
                        if (sponsors.Any()) {
                            foreach (Sponsor sponsor in sponsors) {
                                ev.Sponsors.Add(sponsor);
                            }
                        }
                    }

                    ev.ChannelGroupId = tdo.ChannelGroupId;
                    ev.WebsiteId = tdo.WebsiteId;

                    db.Events.Add(ev);
                }
                catch (Exception ex) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                    this.GenerateException(HttpStatusCode.InternalServerError, ex.Message);
                }

                try {
                    db.SaveChanges();
                }
                catch (Exception ex) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                    this.GenerateException(HttpStatusCode.NotFound, ex.Message);
                }

                // Update the TDO object
                UpdateTDO(tdo, ev);

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, tdo);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { controller = "Events", id = ev.EventId }));
                return response;
            }
            else if (!ModelState.IsValid) {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState.Values.SelectMany(v => v.Errors));
            }
            else {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NoContent));
            }
        }

        // PUT api/Events/Status/1?status=<event status>
        [HttpPut]
        [ActionName("Status")]
        public HttpResponseMessage Status(int id, string status) {
            return UpdateEventStatus(id, (EventStatus)Enum.Parse(typeof(EventStatus), status));
        }

        private void UpdateTDO(EventTDO tdo, Event ev) {
            tdo.EventId = ev.EventId;
            tdo.Created = ev.Created;
            tdo.CreatedBy = ev.CreatedBy;
            tdo.EventVirtualPath = ev.EventVirtualPath;
            tdo.Thumbnail = AppConfigs.VirtualRoot + EventPhotosService.GetThumbnailUrl(ev.EventId, ev.EventFolder, ev.EventVirtualPath, db);
        }

        // DELETE api/Events/1
        public HttpResponseMessage DeleteEvent(int id) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            if (EventPhotosService.IsEventType(ev, EventPhotosService.EVENTTYPE_NEW)) {
                if (EventsController.ClearEventRelationships(ev, db)) {
                    db.Events.Remove(ev);

                    try {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException ex) {
                        FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                        this.GenerateException(HttpStatusCode.NotFound, ex.Message);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, ev);
                }
                else
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Errors.ERROR_EVENT_CLEARREL, ev.EventName));
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(Errors.ERROR_EVENT_DELETE_ANNOTATED, ev.EventName));
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        #region private methods

        private EventTDO GenerateTDO(Event ev = null) {
            EventTDO tdo = null;
            
            IEnumerable<EventOptionTDO> eventOptions = (from evo in db.EventOptions orderby evo.EventOptionName select 
                                           new EventOptionTDO { EventOptionId = evo.EventOptionId, 
                                               EventOptionName = evo.EventOptionName,
                                               SalutationOption = evo.SalutationOption,
                                               FirstNameOption = evo.FirstNameOption,
                                               LastNameOption = evo.LastNameOption,
                                               EmailOption = evo.EmailOption,
                                               AddressOption = evo.AddressOption,
                                               PhoneOption = evo.PhoneOption,
                                               FaxOption = evo.FaxOption,
                                               SignatureOption = evo.SignatureOption
                                           }).ToList();
            IEnumerable<SponsorTDO> sponsors = (from s in db.Sponsors.Where(s => s.User.Id == CurrentUser.Id).OrderBy(s => s.SponsorName) select new SponsorTDO { SponsorId = s.SponsorId, SponsorName = s.SponsorName }).ToList();
            IEnumerable<EmailTemplateTDO> emailTemplates = (from e in db.EmailTemplates.Where(e => e.User.Id == CurrentUser.Id).OrderBy(e => e.EmailTemplateName) select new EmailTemplateTDO { EmailTemplateId = e.EmailTemplateId, EmailTemplateName = e.EmailTemplateName }).ToList();

            if (ev != null)
                tdo = new EventTDO {
                    EventId = ev.EventId,
                    EventName = ev.EventName,
                    EventDescription = ev.EventDescription,
                    EventDate = ev.EventDate,
                    EventLocation = ev.EventLocation,
                    EventFolder = ev.EventFolder,
                    EventVirtualPath = ev.EventVirtualPath,
                    Created = ev.Created,
                    CreatedBy = ev.CreatedBy,
                    EventStatus = ev.EventStatus,
                    PublishAlbum = ev.PublishAlbum,
                    EventOptionId = (ev.EventOption != null) ? ev.EventOption.EventOptionId : 0,
                    SponsorIds = ev.Sponsors.OrderBy(s => s.SponsorName).Select(s => s.SponsorId),
                    EmailTemplateId = (ev.EmailTemplate != null) ? ev.EmailTemplate.EmailTemplateId : 0,
                    ChannelGroupId = ev.ChannelGroupId,
                    WebsiteId = ev.WebsiteId,
                    EventOptions = eventOptions,
                    Sponsors = sponsors,
                    EmailTemplates = emailTemplates
               };
            else
                tdo = new EventTDO {
                    EventDate = DateTime.Now,
                    EventOptions = eventOptions,
                    Sponsors = sponsors,
                    EmailTemplates = emailTemplates
                };

            return tdo;
        }

        private HttpResponseMessage UpdateEventStatus(int id, EventStatus status) {
            Event ev = EventsController.GetEvent(id, CurrentUser.Id, Request, db);
            try {
                ev.EventStatus = (byte)status;
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex) {
                string validationErrors = GetValidationErrors(ex.EntityValidationErrors);
                this.GenerateException(HttpStatusCode.NotFound, validationErrors, _logger);
            }
            catch (DbUpdateConcurrencyException ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                this.GenerateException(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private string GetValidationErrors(IEnumerable<DbEntityValidationResult> eves) {
            var sb = new StringBuilder();
            foreach (var eve in eves) {
                sb.Append(string.Format("{0}: {1} ", eve.Entry.Entity.GetType().Name, eve.Entry.State));
                foreach (var ve in eve.ValidationErrors) {
                    sb.Append(string.Format("{0}: {1} ", ve.PropertyName, ve.ErrorMessage));
                }
            }

            return sb.ToString();
        }
        
        #endregion // private methods
    }
}