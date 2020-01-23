using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils {
    public class Constants {
        public const string HOST_LOCALHOST = "localhost";
        public const string AS_THUMBNAILWIDTH = "ThumbnailWidth";
        
        public const string STR_UNCLAIMED = "Unclaimed";

        public const string FS_API_PREFIX = "fs1";
        public const string FS_AUTHORIZATION_KEY = "FS-Authorization";

        public const string AS_IMAGEEXTS = "ImageExts";
        public const string AS_VIRTUALDIRROOT = "VirtualDirRoot";

        public const string STR_PROCESSED = "Processed";
        public const string STR_THUMB = "Thumb";

        public const string USER_PASSWORD = "**********";

        public const string MEDIATYPE_APPLICATION_JSON = "application/json";
        public const string ASV_SYNCACTION_DEPROVISION = "Deprovision";
        public const string ASV_SYNCACTION_PROVISION = "Provision";
        public const string ASV_SYNCACTION_DEPROVISIONSTORE = "DeprovisionStore";
        
        public const string AS_API_BASEADDRESS = "ApiBaseAddress";
        public const string AS_API_PREFIX = "ApiPrefix";
        public const string AS_API_KEY = "ApiKey";
        public const string AS_USERNAME = "UserName";
        public const string AS_PASSWORD = "Password";
        public const string FS_AUTHORIZATION = "Fs-Authorization";
        public const string PUBLISH_AUTHORIZATION = "C9-API-Key";

        public const string INFO_AUTH_INFO = "Getting auhentication info...";
        
        public const string INFO_EVENT_LIST = "Getting the list of events for the \"{0}\" user on {1}...";
        public const string INFO_EVENT_EVENTOPTIONS = "Getting event options and sponsors for events for the \"{0}\" user...";
        public const string INFO_EVENT_DETAIL = "Getting details of the \"{0}\" event...";
        public const string INFO_EVENT_CREATE = "Creating the \"{0}\" event...";
        public const string INFO_EVENT_UPDATE = "Updating the \"{0}\" event...";
        public const string INFO_EVENT_DELETE = "Deleting the \"{0}\" event...";
        public const string INFO_EVENT_PHOTOS = "Getting the list of photos of the \"{0}\" event...";
        public const string INFO_EVENT_PHOTOSDETAILING = "Getting details of processed photos of the \"{0}\" event...";
        public const string INFO_EVENT_PHOTOSREPORTING = "Getting permission details of processed photos of the \"{0}\" event...";
        public const string INFO_EVENT_PHOTOGROUPS = "Getting the list of photo-groups of the \"{0}\" event ...";
        public const string INFO_EVENT_AUTHORIZEDPHOTOS = "Getting the list of publish-authorized photos of the \"{0}\" event...";
        public const string INFO_EVENT_UNAUTHORIZEDPHOTOS = "Getting the list of non-publish-authorized photos of the \"{0}\" event...";
        public const string INFO_EVENT_AUTHORIZEDPHOTOS_NOPHOTOS = "There is no publish-authorized photo for this event.";
        public const string INFO_EVENT_NUMPROCESSEDPHOTOS = "Getting the number of processed photos of the \"{0}\" event ...";

        public const string INFO_EVENTOPTION_LIST = "Getting the list of event options...";
        public const string INFO_EVENTOPTION_DETAIL = "Getting details of the \"{0}\" event option...";
        public const string INFO_EVENTOPTION_CREATE = "Creating the \"{0}\" event option...";
        public const string INFO_EVENTOPTION_UPDATE = "Updating the \"{0}\" event option...";
        public const string INFO_EVENTOPTION_DELETE = "Deleting the \"{0}\" event option...";

        public const string INFO_ACCOUNT_DETAIL = "Getting details of the \"{0}\" account...";

        public const string INFO_PUBLISHCONFIG = "Setting publish configuration for the \"{0}\" user....";
        public const string INFO_PUBLISHCONFIG_SUCCESS = "Updated publish info successfully.";
        public const string INFO_PUBLISHCONFIG_DETAIL = "Getting details of the publish config of the \"{0}\" user...";

        public const string INFO_EMAILSERVERCONFIG = "Setting email server configuration for the \"{0}\" user....";
        public const string INFO_EMAILSERVERCONFIG_SUCCESS = "Updated email server configuration successfully.";
        public const string INFO_EMAILSERVERCONFIG_DETAIL = "Getting details of the email server config of the \"{0}\" user...";

        public const string INFO_EVENTSPONSOR_LIST = "Getting the list of event sponsors for the \"{0}\" user...";
        public const string INFO_EVENTSPONSOR_DETAIL = "Getting details of the \"{0}\" event sponsor...";
        public const string INFO_EVENTSPONSOR_CREATE = "Creating the \"{0}\" event sponsor...";
        public const string INFO_EVENTSPONSOR_UPDATE = "Updating the \"{0}\" event sponsor...";
        public const string INFO_EVENTSPONSOR_DELETE = "Deleting the \"{0}\" event sponsor...";

        public const string INFO_EMAILTEMPLATE_LIST = "Getting the list of email templates for the \"{0}\" user...";
        public const string INFO_EMAILTEMPLATE_DETAIL = "Getting details of the \"{0}\" email template...";
        public const string INFO_EMAILTEMPLATE_CREATE = "Creating the \"{0}\" email template...";
        public const string INFO_EMAILTEMPLATE_UPDATE = "Updating the \"{0}\" email template...";
        public const string INFO_EMAILTEMPLATE_DELETE = "Deleting the \"{0}\" email template...";

        // Publishing
        public const string INFO_CHANNELGROUP_LIST = "Getting the list of channel groups...";

        public const string INFO_SUCCEEDED = "Succeeded";

        // Actions
        public const string ACTION_EVENT_MODIFY = "ModifyEvent";

        // TABLES
        public const string TABLE_SPONSORS = "Sponsors";
        public const string TABLE_EMAILTEMPLATES = "EmailTemplates";
        public const string TABLE_GUESTS = "Guests";
        public const string TABLE_PHOTOS = "Photos";
        public const string TABLE_GUESTPHOTOES = "GuestPhotoes";

        public const string TABLE_ACCOUNTS = "Accounts";
        public const string TABLE_USERROLES = "UserRoles";
        public const string TABLE_USERAUTHORIZATIONS = "UserAuthorizations";
        public const string TABLE_USERS = "Users";
        public const string TABLE_EVENTOPTIONS = "EventOptions";
        public const string TABLE_EVENTS = "Events";
    }
}
