using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Net;

using FotoShoutUtils.Utils;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using FotoShoutUtils.Formatters;
using FotoShoutUtils.Service;
using FotoShoutPublishingService.Services;
using FotoShoutPublishingService.Services.Email;
using FotoShoutData.Models;
using FotoShoutData.Models.Authenticate;
using FotoShoutPublishingService.Db;
using FotoShoutData.Models.Publish;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;

namespace FotoShoutPublishingService {
    class PublishingEngine: Executor {
        static ILog _logger = LogManager.GetLogger(typeof(PublishingEngine));

        private PublishApiWebService _c9WebService = null;
        private FsApiWebService _fsWebService = new FsApiWebService(AppSettings.FsApiBaseAddress, AppSettings.FsApiPrefix, FotoShoutUtils.Constants.MEDIATYPE_APPLICATION_JSON);

        private EmailService _emailService = new EmailService();

        private UserTDO User { get; set; }

        public PublishingEngine() {
        }

        public int PublishDelay { get; set; }

        public override bool Initialize(bool timerUsed) {
            FotoShoutUtils.Log.LogManager.Info(_logger, Assembly.GetExecutingAssembly().GetName().Version);
            FotoShoutUtils.Log.LogManager.Info(_logger, "Initializing...");

            if (!base.Initialize(timerUsed))
                return false;

            FotoShoutUtils.Log.LogManager.Info(_logger, "Done Initalizing process.\r\n");
            
            return true;
        }
        
        /// <summary>
        /// Process publishing on timer
        /// </summary>
        public override void Execute() {
            try {
                PublishDelay = AppSettings.PublishDelay;
                
                IEnumerable<FotoShoutData.Models.Credentials> credentials = UsersRepo.Credentials;
                if (credentials != null) {
                    foreach (FotoShoutData.Models.Credentials credential in credentials) {
                        if (credential.Email.Equals("phongnguyen17@yahoo.com")) {
                            if (Authenticate(credential)) {
                                PublishEvents();
                                FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Published all events of the \"{0}\" user.", credential.Email));
                            }
                        }
                    }
                }
            }
            catch (HttpClientServiceException ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("HTTP request failed\r\nStatus Code: {0} - {1}\r\n{2}\r\n", (int)ex.StatusCode, ex.Message, ex.ToString()));
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString() + "\r\n");
            }
        }

        private void PublishEvents() {
            try {
                _emailService.EmailServerAccount = _fsWebService.GetEmailServerConfiguration();
                if (!_emailService.IsValid())
                    FotoShoutUtils.Log.LogManager.Error(_logger, "There is not enough email server info, so there will be no email sent out to guests for this event.\r\n");
            }
            catch (HttpClientServiceException ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.Message + "\r\n");
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString() + "\r\n");
            }
            
            try {
                IEnumerable<EventTDO> tdos = _fsWebService.GetEvents("Open");
                if (!tdos.Any()) {
                    FotoShoutUtils.Log.LogManager.Info(_logger, "There is no opening events for the current user.\r\n");
                    return;
                }

                foreach (EventTDO tdo in tdos) {
                    EventTDO ev = _fsWebService.GetEvent(tdo.EventId);
                    PostProcessEvent(ev);
                    PublishEvent(ev);
                }
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString() + "\r\n");
            }
        }

        private bool Authenticate(FotoShoutData.Models.Credentials credential) {
            try {
                FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Authenticating for the \"{0}\" user...", credential.Email));
                User = _fsWebService.Login(credential);
                if (User == null) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, "Unexpected error - There is no user on the central fotoShout associated with the fotoShout Authorization key.\r\n");
                    return false;
                }
                FotoShoutUtils.Log.LogManager.Info(_logger, "Succeeded logged");
                FotoShoutUtils.Log.LogManager.Info(_logger, "Getting publish configuration information...");
                PublishAccount publishAccount = _fsWebService.GetPublishConfiguration();
                if (publishAccount != null && !string.IsNullOrEmpty(publishAccount.Url)) {
                    string url = publishAccount.Url.EndsWith("/") ? publishAccount.Url.Substring(0, publishAccount.Url.Length - 1) : publishAccount.Url;
                    int lastIdx = url.LastIndexOf("/");
                    if (lastIdx != -1) {
                        string baseAddress = url.Substring(0, lastIdx + 1);
                        string prefix = url.Substring(lastIdx + 1);
                        _c9WebService = new PublishApiWebService(baseAddress, prefix, FotoShoutUtils.Constants.MEDIATYPE_APPLICATION_JSON);
                        string ret = _c9WebService.Authenticate(publishAccount.ApiKey.ToString(), new LoginModel {
                            UserName = publishAccount.Email,
                            Password = publishAccount.Password
                        });

                        if (string.IsNullOrEmpty(ret)) {
                            FotoShoutUtils.Log.LogManager.Error(_logger, "Unable to authorize to the publishing API.\r\n");
                            return false;
                        }
                    }
                    else {
                        FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Unable to authorize to the publishing API - The \"{0}\" url is incorrect.\r\n", publishAccount.Url));
                        return false;
                    }
                }
                else {
                    FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("There is no publish configuration for the \"{0}\" user.\r\n", publishAccount.Url));
                    return false;
                }

                FotoShoutUtils.Log.LogManager.Info(_logger, "Successfully authenticated.\r\n");

                return true;
            }
            catch (HttpClientServiceException ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Failed to authenticate : {0} (Status Code: {1})\r\n", ex.Message, (int)ex.StatusCode));
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Failed to authenticate: {0}\r\n", ex.ToString()));
            }

            return false;
        }

        private void PostProcessEvent(EventTDO ev) {
            try {
                _emailService.EmailTemplate = null;
                if (_emailService.IsValid()) {
                    if (ev.EmailTemplateId <= 0) {
                        FotoShoutUtils.Log.LogManager.Error(_logger, "There is no email template assigned to the event, so there will be no email sent out to guests for this event.");
                        return;
                    }
                    else {
                        _emailService.EmailTemplate = _fsWebService.GetEmailTemplate((int)ev.EmailTemplateId);
                    }
                }

                IEnumerable<PhotoTDO> unauthorizedPhotos = this._fsWebService.GetUnauthorizedPhotos(ev.EventId);
                foreach (PhotoTDO photo in unauthorizedPhotos) {
                    PhotoAnnotation photoAnnotation = this._fsWebService.Get<PhotoAnnotation>("PhotoAnnotation/" + photo.PhotoId, true);
                    if (photoAnnotation != null && photoAnnotation.Guests.Any<GuestTDO>() && this._emailService.EmailTemplate != null)
                        this.SendEmails(this.User, ev, photo, photoAnnotation, photo.Image);
                }

                FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Getting broadcasts of the {0} event...", ev.EventName));
                IEnumerable<EventBroadcast> broadcasts = _fsWebService.GetList<EventBroadcast>("EventBroadcasts/" + ev.EventId, true);
                if (!broadcasts.Any()) {
                    FotoShoutUtils.Log.LogManager.Info(_logger, "There is no broadcast published recently.");
                    return;
                }

                foreach (EventBroadcast bc in broadcasts) {
                    PhotoAnnotation photoAnnotation = _fsWebService.Get<PhotoAnnotation>("PhotoAnnotation/" + bc.PhotoId, true);
                    if (photoAnnotation != null && photoAnnotation.Guests.Any()) {
                        FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Getting the permalinks for the broadcast {0}.", bc.BroadcastId));
                        try {
                            IDictionary<string, string> permalinksDict = _c9WebService.GetPermaLinks(bc.BroadcastId);
                            if (!permalinksDict.Any())
                                FotoShoutUtils.Log.LogManager.Debug(_logger, string.Format("The broadcast with the id of {0} does not provide any perma-link. It might not be published yet.", bc.BroadcastId));
                            else {
                                bool updated = false;
                                ICollection<string> newPermaLinks = new List<string>();
                                updated = UpdatePermaLinks(bc, permalinksDict, newPermaLinks);
                                if (_emailService.EmailTemplate != null && updated) {
                                    _emailService.SendEmails(User, ev, photoAnnotation, newPermaLinks);
                                    bc.Error = _emailService.Error;
                                    if (string.IsNullOrEmpty(_emailService.Error))
                                        bc.Status = permalinksDict.ContainsKey("pending") ? (byte)EventBroadcastStatus.PublishPending : (byte)EventBroadcastStatus.Processed;
                                    else
                                        bc.Error = _emailService.Error;
                                    updated = true;
                                }
                                else if (updated)
                                    bc.Status = permalinksDict.ContainsKey("pending") ? (byte)EventBroadcastStatus.PublishPending : (byte)EventBroadcastStatus.Processed;
                                if (updated)
                                    _fsWebService.UploadString("EventBroadcasts?broadcastId=" + bc.EventBroadcastId, GeneratePostContent<EventBroadcast>(bc), "PUT");
                            }
                        }
                        catch (Exception ex) {
                            FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
            }
        }

        private void SendEmails(UserTDO user, EventTDO ev, PhotoTDO photo, PhotoAnnotation photoAnnotation, string photoUrl) {
            if (ev.WebsiteId != null) {
                Website website = _fsWebService.Get<Website>("Websites/" + ev.WebsiteId, true);
                photoUrl = GeneratePhotoWebsiteUrl(photoUrl, website); // string.Format("{0}PhotoWebsite/{1}?website={2}", Regex.Replace(AppSettings.FsApiBaseAddress, @"api/", ""), photo.PhotoId, ev.WebsiteId);
            }

            ICollection<GuestTDO> guests = photoAnnotation.Guests;
            IEnumerable<PhotoEmail> list = this._fsWebService.GetList<PhotoEmail>("PhotoEmails/" + (object)photo.PhotoId, true);
            foreach (GuestTDO guestTdo in (IEnumerable<GuestTDO>)guests) {
                GuestTDO guest = guestTdo;
                bool flag = true;
                PhotoEmail postObject = list.Where(ee => ee.PhotoId == photo.PhotoId && ee.GuestId == guest.GuestId).FirstOrDefault();
                if (postObject == null) {
                    postObject = new PhotoEmail {
                        EventId = ev.EventId,
                        PhotoId = photo.PhotoId,
                        GuestId = guest.GuestId
                    };
                    flag = false;
                }
                else if (postObject.Status == (byte)1)
                    continue;
                this._emailService.Error = "";
                this._emailService.SendEmailTo(user, ev, guest, photoUrl);
                if (string.IsNullOrEmpty(this._emailService.Error)) {
                    postObject.Status = (byte)1;
                    postObject.Error = null;
                }
                else {
                    postObject.Status = byte.MaxValue;
                    postObject.Error = this._emailService.Error;
                }
                if (flag)
                    this._fsWebService.UploadString("PhotoEmails?photoEmailId=" + (object)postObject.PhotoEmailId, this.GeneratePostContent<PhotoEmail>(postObject), "PUT");
                else
                    this._fsWebService.UploadString("PhotoEmails", this.GeneratePostContent<PhotoEmail>(postObject), null);
            }
        }

        private bool UpdatePermaLinks(EventBroadcast bc, IDictionary<string, string> permaLinks, ICollection<string> newPermaLinks) {
            string thumbnails = "";
            string links = "";
            string[] bcPermaLinks = string.IsNullOrEmpty(bc.PermaLinks) ? null : bc.PermaLinks.Split('|');
            foreach (KeyValuePair<string, string> kv in permaLinks) {
                if (!kv.Key.Equals("pending", StringComparison.InvariantCultureIgnoreCase)) {
                    if (!string.IsNullOrEmpty(thumbnails))
                        thumbnails += "|";
                    thumbnails += kv.Key;
                    if (!string.IsNullOrEmpty(links))
                        links += "|";
                    links += kv.Value;
                    if (bcPermaLinks == null || !bcPermaLinks.Contains(kv.Value)) {
                        newPermaLinks.Add(kv.Value);
                    }
                }
            }
            if (newPermaLinks.Any()) {
                bc.PermaLinks = links;
                bc.Thumbnails = thumbnails;
                FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Updated the permalinks for the broadcast {0}.", bc.BroadcastId));
                return true;
            }

            return false;
        }

        private void PublishEvent(EventTDO ev) {
            try {
                if (ev.ChannelGroupId == 0 || ev.ChannelGroupId == null) {
                    FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("There is no publish channel group being configured for the \"{0}\" event, so no photos will be published for this event.", ev.EventName));
                    return;
                }
                    
                FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Getting details of the channel group {0}...", ev.ChannelGroupId));
                ChannelGroup channelGroup = _c9WebService.Get<ChannelGroup>("Template/" + ev.ChannelGroupId, true);
                if (channelGroup == null)
                    FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("There is no channel group with the id {0}.", ev.ChannelGroupId));

                IEnumerable<PhotoTDO> photos = _fsWebService.GetAuthorizedPhotos(ev.EventId);
                if (!photos.Any()) {
                    FotoShoutUtils.Log.LogManager.Info(_logger, FotoShoutUtils.Constants.INFO_EVENT_AUTHORIZEDPHOTOS_NOPHOTOS);
                    return;
                }
                foreach (PhotoTDO photo in photos) {
                    PublishPhoto(ev, photo, channelGroup);
                    Thread.Sleep(PublishDelay * 1000);
                    //await PublishDelayAsync();
                }
            }
            catch (HttpClientServiceException ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("HTTP request failed\r\nStatus Code: {0} - {1}\r\n{2}", (int)ex.StatusCode, ex.Message, ex.ToString()));
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
            }
        }

        //private async Task<string> PublishDelayAsync() {
        //    await Task.Delay(PublishDelay * 1000);
        //    return "Done";
        //}

        private void PublishPhoto(EventTDO ev, PhotoTDO photo, ChannelGroup channelGroup) {
            bool pending = false;
            bool publishedPhoto = false;
            bool published = false;
            try {
                this.Error = "";
                string filename = photo.Folder.EndsWith(Path.DirectorySeparatorChar.ToString()) ? photo.Folder : (photo.Folder + Path.DirectorySeparatorChar) + Constants.STR_PROCESSED + Path.DirectorySeparatorChar + photo.Filename;
                if (File.Exists(filename)) {
                    HttpResponseMessage response = _fsWebService.Put("EventPhotos/PendingPublish/" + ev.EventId + "?filename=" + photo.Filename, null, true);
                    if (!response.IsSuccessStatusCode)
                        FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Failed to mark the {0} as pending publish.\r\nStatus Code: {0} - {1}\r\n{2}", (int)response.StatusCode, response.ReasonPhrase));
                    pending = true;

                    PublishPhotoToChannel(ev, photo, channelGroup);
                    
                    publishedPhoto = true;

                    response = _fsWebService.Put("EventPhotos/Published/" + ev.EventId + "?filename=" + photo.Filename, null, true);
                    if (!response.IsSuccessStatusCode)
                        FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Failed to mark the {0} as published.\r\nStatus Code: {0} - {1}\r\n{2}", (int)response.StatusCode, response.ReasonPhrase));
                    published = true;
                }
                else {
                    FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("{0} has not been uploaded. It will be published once uploaded.", filename));
                }
            }
            catch (HttpClientServiceException ex) {
                this.Error = string.Format("HTTP request failed\r\nStatus Code: {0} - {1}\r\n{2}", (int)ex.StatusCode, ex.Message, ex.ToString());
                FotoShoutUtils.Log.LogManager.Error(_logger, this.Error);
            }
            catch (Exception ex) {
                this.Error = ex.ToString();
                FotoShoutUtils.Log.LogManager.Error(_logger, this.Error);
            }
            finally {
                if (!published) {
                    if (publishedPhoto)
                        FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("The {0} photo of the {1} event is published. However, it failed to mark the photo as published.", photo.Filename, ev.EventName));
                    else if (pending) {
                        try {
                            _fsWebService.Put("EventPhotos/UnpendingPublish/" + ev.EventId + "?filename=" + photo.Filename + "&error=" + this.Error, null, true);
                        }
                        catch (Exception ex) {
                            FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString());
                        }
                    }
                }
            }
        }

        private void PublishPhotoToChannel(EventTDO ev, PhotoTDO photo, ChannelGroup channelGroup) {
            if (channelGroup.Fields == null || !channelGroup.Fields.Any()) {
                FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("There is no broadcast fields in the {0} template.", channelGroup.Name));
                return;
            }
            
            PhotoAnnotation annotation = _fsWebService.Get<PhotoAnnotation>("PhotoAnnotation/" + photo.PhotoId, true);

            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Publishing the {0} photo to the {1} channel group...", photo.Filename, string.IsNullOrEmpty(channelGroup.Name) ? channelGroup.ID.ToString() : channelGroup.Name));

            string filename = photo.Folder.EndsWith(Path.DirectorySeparatorChar.ToString()) ? photo.Folder : (photo.Folder + Path.DirectorySeparatorChar) + Constants.STR_PROCESSED + Path.DirectorySeparatorChar + photo.Filename;
            PostBroadcast bc = new PostBroadcast {
                TemplateID = channelGroup.ID,
                Status = "Pending Publish",
                Description = "FotoShout Broadcast",
                Name = Path.GetFileNameWithoutExtension(photo.Filename),
                ScheduledTime = DateTime.Now.ToShortDateString(),
                BroadcastFields = new List<BroadcastFieldValue>()
            };
            foreach (BroadcastField bf in channelGroup.Fields) {
                if (bf.Name.Equals("Album Title", StringComparison.InvariantCultureIgnoreCase)) {
                    BroadcastFieldValue bfv = new BroadcastFieldValue {
                        IdToken = bf.IdToken,
                        Value = string.IsNullOrEmpty(ev.PublishAlbum) ? ev.EventName : ev.PublishAlbum
                    };
                    bc.BroadcastFields.Add(bfv);
                }
                else if (bf.Name.Equals("Album Description", StringComparison.InvariantCultureIgnoreCase)) {
                    BroadcastFieldValue bfv = new BroadcastFieldValue {
                        IdToken = bf.IdToken,
                        Value = string.IsNullOrEmpty(ev.PublishAlbum) ? ev.EventName : ev.PublishAlbum
                    };
                    bc.BroadcastFields.Add(bfv);
                }
                else if (bf.Name.Equals("Upload Photos", StringComparison.InvariantCultureIgnoreCase)) {
                    string serverUri = _c9WebService.UploadFile("Media/Upload", filename);
                    if (string.IsNullOrEmpty(serverUri))
                        throw new PublishingException(string.Format("Unexpected error: Can not upload the {0} photo.", photo.Filename));
                    BroadcastFieldValue bfv = new BroadcastFieldValue {
                        IdToken = bf.IdToken,
                        Value = serverUri
                    };
                    bc.BroadcastFields.Add(bfv);
                }
                else if (bf.Name.Equals("Title", StringComparison.InvariantCultureIgnoreCase)) {
                    BroadcastFieldValue bfv = new BroadcastFieldValue {
                        IdToken = bf.IdToken,
                        Value = bc.Name
                    };
                    bc.BroadcastFields.Add(bfv);
                }
                else if (bf.Name.Equals("Image File", StringComparison.InvariantCultureIgnoreCase)) {
                    string serverUri = _c9WebService.UploadFile("Media/Upload", filename);
                    if (string.IsNullOrEmpty(serverUri))
                        throw new PublishingException(string.Format("Unexpected error: Can not upload the {0} photo.", photo.Filename));
                    BroadcastFieldValue bfv = new BroadcastFieldValue {
                        IdToken = bf.IdToken,
                        Value = serverUri
                    };
                    bc.BroadcastFields.Add(bfv);
                }
                else if (bf.Name.Equals("File", StringComparison.InvariantCultureIgnoreCase)) {
                    string serverUri = _c9WebService.UploadFile("Media/Upload", filename);
                    if (string.IsNullOrEmpty(serverUri))
                        throw new PublishingException(string.Format("Unexpected error: Can not upload the {0} photo.", photo.Filename));
                    BroadcastFieldValue bfv = new BroadcastFieldValue {
                        IdToken = bf.IdToken,
                        Value = serverUri
                    };
                    bc.BroadcastFields.Add(bfv);
                }
            }

            string ret = _c9WebService.UploadString("Broadcast", GeneratePostContent<PostBroadcast>(bc));
            if (!string.IsNullOrEmpty(ret)) {
                EventBroadcast eventBroadcast = new EventBroadcast {
                    BroadcastId = int.Parse(ret.Trim(new char[] { '\"' })),
                    EventId = ev.EventId,
                    PhotoId = photo.PhotoId,
                    Status = 0
                };
                ret = _fsWebService.UploadString("EventBroadcasts", GeneratePostContent<EventBroadcast>(eventBroadcast));
            }
            else
                throw new PublishingException(string.Format("Unsuccessully publishing the \"{0}\" photo using the \"{1}\" channel group.", photo.Filename, channelGroup.Name));
            
            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Published the {0} photo.", photo.Filename));
        }

        private string GeneratePhotoWebsiteUrl(string photoUrl, Website website) {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("photo={0}", WebUtility.UrlEncode(photoUrl)));
            if (website.HeaderImage != null) {
                sb.Append(string.Format("&hi={0}", WebUtility.UrlEncode(website.HeaderImage)));
            }
            if (website.HeaderUrl != null) {
                sb.Append(string.Format("&hu={0}", WebUtility.UrlEncode(website.HeaderUrl)));
            }
            if (website.TopInfoBlockImage != null) {
                sb.Append(string.Format("&ti={0}", WebUtility.UrlEncode(website.TopInfoBlockImage)));
            }
            if (website.TopInfoBlockUrl != null) {
                sb.Append(string.Format("&tu={0}", WebUtility.UrlEncode(website.TopInfoBlockUrl)));
            }
            if (website.BottomInfoBlockImage != null) {
                sb.Append(string.Format("&bi={0}", WebUtility.UrlEncode(website.BottomInfoBlockImage)));
            }
            if (website.BottomInfoBlockUrl != null) {
                sb.Append(string.Format("&bu={0}", WebUtility.UrlEncode(website.BottomInfoBlockUrl)));
            }
            if (website.FooterImage != null) {
                sb.Append(string.Format("&fi={0}", WebUtility.UrlEncode(website.FooterImage)));
            }
            if (website.FooterUrl != null) {
                sb.Append(string.Format("&fu={0}", WebUtility.UrlEncode(website.FooterUrl)));
            }

            return string.Format("{0}PhotoWebsite?{1}", Regex.Replace(AppSettings.FsApiBaseAddress, @"api/", ""), sb.ToString());
        }

        private string GeneratePostContent<T>(T postObject) {
            return JsonConvert.SerializeObject(postObject);
        }
    }
}
