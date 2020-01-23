using FotoShoutData.Models;
using FotoShoutData.Models.Authenticate;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace FotoShoutUtils.Service {
    public class FsApiWebService: AuthenticationWebService {
        static ILog _logger = LogManager.GetLogger(typeof(FsApiWebService));
        
        protected UserTDO _user = null;

        public FsApiWebService(string baseAddress, string prefix, string mediaType) 
            : base(baseAddress, prefix, mediaType, _logger) {
            AuthorizationHeader = Constants.FS_AUTHORIZATION;
        }

        public UserTDO Login(Credentials credentials) {
            string ret = Authenticate(credentials);
            if (!string.IsNullOrEmpty(ret)) {
                FotoShoutUtils.Log.LogManager.Debug(_logger, "Getting user info...");
                // Need to switch back
                _user = GetUser<UserTDO>();
                //_user = new UserTDO {
                //    FirstName = "Phong",
                //    LastName = "Nguyen"
                //};
            }

            return _user;
        }

        public UserTDO Login(string apiKey, LoginModel model) {
            return Login(GenerateCredentials(apiKey, model) as Credentials);
        }

        public string GetAuthId() {
            return Get<string>("Authorization?type=id", Constants.INFO_AUTH_INFO);
        }

        public IEnumerable<EventTDO> GetEvents(string eventType) {
            return GetList<EventTDO>("Events" + (!string.IsNullOrEmpty(eventType) ? ("?eventType=" + eventType) : ""), string.Format(Constants.INFO_EVENT_LIST, (_user != null) ? _user.Email : "", BaseAddress));
        }

        public EventTDO GetEvent(int id) {
            return Get<EventTDO>("Events/", id, Constants.INFO_EVENT_DETAIL);
        }

        public EventTDO CreateEvent(EventTDO ev) {
            return Modify<EventTDO>("Events", ev, string.Format(Constants.INFO_EVENT_CREATE, ev.EventName));
        }

        public EventTDO UpdateEvent(int id, EventTDO ev) {
            return Modify<EventTDO>("Events/" + id.ToString(), ev, string.Format(Constants.INFO_EVENT_UPDATE, ev.EventName), "PUT");
        }

        public HttpResponseMessage ChangeEventStatus(int id, string evStatus) {
            return Put("Events/Status/" + id.ToString() + "?status=" + evStatus, null, true);
        }

        public EventTDO DeleteEvent(int id) {
            return Delete<EventTDO>("Events", id, Constants.INFO_EVENT_DELETE);
        }
        
        public IEnumerable<PhotoTDO> GetPhotos(int id) {
            return GetList<PhotoTDO>("EventPhotos/Photos/" + id.ToString(), string.Format(Constants.INFO_EVENT_PHOTOS, id));
        }

        public IEnumerable<PhotoTDO> GetProcessedPhotos(int id, string created = "", int page = 0, int pageSize = 12) {
            return GetList<PhotoTDO>(string.Format("EventPhotos/ProcessedPhotos/{0}?created={1}&page={2}&pageSize={3}", id, created, page, pageSize), string.Format(Constants.INFO_EVENT_PHOTOS, id));
        }

        public IEnumerable<PhotoDetailsTDO> GetProcessedPhotosDetailing(int id, string created = "", int page = 0, int pageSize = 12) {
            return GetList<PhotoDetailsTDO>(string.Format("EventPhotos/ProcessedPhotosDetailing/{0}?created={1}&page={2}&pageSize={3}", id, created, page, pageSize), string.Format(Constants.INFO_EVENT_PHOTOSDETAILING, id));
        }

        public EventDetailsTDO GetProcessedPhotosReporting(int id) {
            return Get<EventDetailsTDO>(string.Format("EventPhotos/ProcessedPhotosReporting/{0}", id), string.Format(Constants.INFO_EVENT_PHOTOSREPORTING, id));
        }

        public int GetNumProcessedPhotos(int id, string created = "") {
            return Get<int>("EventPhotos/NumProcessedPhotos/" + id.ToString() + (string.IsNullOrEmpty(created) ? "" : ("?created=" + created)), string.Format(Constants.INFO_EVENT_NUMPROCESSEDPHOTOS, id));
        }
        
        public IEnumerable<PhotoGroupTDO> GetProcessedPhotoGroupsByDate(int id) {
            return GetList<PhotoGroupTDO>("EventPhotos/ProcessedPhotoGroupsByDate/" + id.ToString(), string.Format(Constants.INFO_EVENT_PHOTOGROUPS, id));
        }
        
        public IEnumerable<PhotoTDO> GetAuthorizedPhotos(int id) {
            return GetList<PhotoTDO>("EventPhotos/PublishAuthorizedPhotos/" + id.ToString(), string.Format(Constants.INFO_EVENT_AUTHORIZEDPHOTOS, id));
        }

        public IEnumerable<PhotoTDO> GetUnauthorizedPhotos(int id) {
            return this.GetList<PhotoTDO>("EventPhotos/PublishUnauthorizedPhotos/" + id.ToString(), string.Format(Constants.INFO_EVENT_UNAUTHORIZEDPHOTOS, id));
        }

        public PublishAccount GetPublishConfiguration() {
            return Get<PublishAccount>("PublishConfig", string.Format(Constants.INFO_PUBLISHCONFIG_DETAIL, (_user != null) ? _user.Email : ""));
        }

        public Account GetAccount() {
            return Get<Account>("Accounts?name=" + _user.AccountName, string.Format(Constants.INFO_ACCOUNT_DETAIL, (_user != null) ? _user.AccountName : ""));
        }

        public PublishAccount PublishConfiguration(PublishAccount model) {
            return (model.Id != 0) ? 
                Modify<PublishAccount>("PublishConfig/" + model.Id, model, Constants.INFO_PUBLISHCONFIG, "PUT") :
                Modify<PublishAccount>("PublishConfig", model, Constants.INFO_PUBLISHCONFIG);
        }

        public EmailServerAccount GetEmailServerConfiguration() {
            return Get<EmailServerAccount>("EmailServerConfig", string.Format(Constants.INFO_EMAILSERVERCONFIG_DETAIL, (_user != null) ? _user.Email : ""));
        }

        public EmailServerAccount EmailServerConfiguration(EmailServerAccount model) {
            return (model.EmailServerAccountId != 0) ?
                Modify<EmailServerAccount>("EmailServerConfig/" + model.EmailServerAccountId, model, Constants.INFO_EMAILSERVERCONFIG, "PUT") :
                Modify<EmailServerAccount>("EmailServerConfig", model, Constants.INFO_EMAILSERVERCONFIG);
        }

        public IEnumerable<EventOption> GetEventOptions() {
            return GetList<EventOption>("EventOptions", string.Format(Constants.INFO_EVENTOPTION_LIST, (_user != null) ? _user.Email : ""));
        }

        public EventOption GetEventOption(int id) {
            return Get<EventOption>("EventOptions/", id, Constants.INFO_EVENTOPTION_DETAIL);
        }

        public object CreateEventOption(EventOption evOption) {
            return Modify<EventOption>("EventOptions", evOption, string.Format(Constants.INFO_EVENTOPTION_CREATE, evOption.EventOptionName));
        }

        public object UpdateEventOption(EventOption evOption) {
            return Modify<EventOption>("EventOptions/" + evOption.EventOptionId, evOption, string.Format(Constants.INFO_EVENTOPTION_UPDATE, evOption.EventOptionName), "PUT");
        }

        public EventOption DeleteEventOption(int id) {
            return Delete<EventOption>("EventOptions", id, Constants.INFO_EVENTOPTION_DELETE);
        }
        
        public IEnumerable<Sponsor> GetEventSponsors() {
            return GetList<Sponsor>("Sponsors", string.Format(Constants.INFO_EVENTSPONSOR_LIST, (_user != null) ? _user.Email : ""));
        }

        public Sponsor GetEventSponsor(int id) {
            return Get<Sponsor>("Sponsors/", id, Constants.INFO_EVENTSPONSOR_DETAIL);
        }

        public object CreateEventSponsor(Sponsor sponsor) {
            return Modify<Sponsor>("Sponsors", sponsor, string.Format(Constants.INFO_EVENTSPONSOR_CREATE, sponsor.SponsorName));
        }

        public object UpdateEventSponsor(Sponsor sponsor) {
            return Modify<Sponsor>("Sponsors/" + sponsor.SponsorId, sponsor, string.Format(Constants.INFO_EVENTSPONSOR_UPDATE, sponsor.SponsorName), "PUT");
        }

        public Sponsor DeleteEventSponsor(int id) {
            return Delete<Sponsor>("Sponsors", id, Constants.INFO_EVENTSPONSOR_DELETE);
        }

        public IEnumerable<EmailTemplate> GetEmailTemplates() {
            return GetList<EmailTemplate>("EmailTemplates", string.Format(Constants.INFO_EMAILTEMPLATE_LIST, (_user != null) ? _user.Email : ""));
        }

        public EmailTemplate GetEmailTemplate(int id) {
            return Get<EmailTemplate>("EmailTemplates/", id, Constants.INFO_EMAILTEMPLATE_DETAIL);
        }

        public object CreateEmailTemplate(EmailTemplate emailTemplate) {
            return Modify<EmailTemplate>("EmailTemplates", emailTemplate, string.Format(Constants.INFO_EMAILTEMPLATE_CREATE, emailTemplate.EmailTemplateName));
        }

        public object UpdateEmailTemplate(EmailTemplate emailTemplate) {
            return Modify<EmailTemplate>("EmailTemplates/" + emailTemplate.EmailTemplateId, emailTemplate, string.Format(Constants.INFO_EMAILTEMPLATE_UPDATE, emailTemplate.EmailTemplateName), "PUT");
        }

        public EmailTemplate DeleteEmailTemplate(int id) {
            return Delete<EmailTemplate>("EmailTemplates", id, Constants.INFO_EMAILTEMPLATE_DELETE);
        }

        protected override Object GenerateCredentials(string apiKey, LoginModel model) {
            return new FotoShoutData.Models.Credentials { APIKey = apiKey, Email = model.UserName, Password = model.Password };
        }
    }
}