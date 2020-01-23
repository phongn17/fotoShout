using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutUtils {
    public class Errors {
        public const string ERROR_UNSPECIFIED = "Unspecified error";
        public const string ERROR_AUTH_NOUSER = "Unexpected error - There is no user associated with the FotoShout Authorization key";
        public const string ERROR_CREDENTIALS_INVALID = "The user name or password provided is incorrect.";
        public const string ERROR_EVENT_EVENTOPTIONS = "Cannot get event options and sponsors for events.";
        public const string ERROR_EVENT_DELETE = "Unexpected error when deleting the \"{0}\" event.";
        public const string ERROR_EVENT_COMPLETE = "Unexpected error when marking the \"{0}\" event as complete.";
        public const string ERROR_EVENT_DETAIL = "Cannot get details of the \"{0}\" event.";
        public const string ERROR_EVENT_CREATE_UNEXPECTED = "Unexpected error when creating the \"{0}\" event.";
        public const string ERROR_EVENT_UPDATE_UNEXPECTED = "Unexpected error when updating the \"{0}\" event.";

        public const string ERROR_EVENTOPTION_DELETE = "Unexpected error when deleting the \"{0}\" event option.";
        public const string ERROR_EVENTOPTION_DETAIL = "Cannot get details of the \"{0}\" event option.";
        public const string ERROR_EVENTOPTION_CREATE_UNEXPECTED = "Unexpected error when creating the \"{0}\" event option.";
        public const string ERROR_EVENTOPTION_UPDATE_UNEXPECTED = "Unexpected error when updating the \"{0}\" event option.";
        
        public const string ERROR_EVENTSPONSOR_DELETE = "Unexpected error when deleting the \"{0}\" event sponsor.";
        public const string ERROR_EVENTSPONSOR_DETAIL = "Cannot get details of the \"{0}\" event sponsor.";
        public const string ERROR_EVENTSPONSOR_CREATE_UNEXPECTED = "Unexpected error when creating the \"{0}\" event sponsor.";
        public const string ERROR_EVENTSPONSOR_UPDATE_UNEXPECTED = "Unexpected error when updating the \"{0}\" event sponsor.";
        
        public const string ERROR_EMAILTEMPLATE_DELETE = "Unexpected error when deleting the \"{0}\" email template.";
        public const string ERROR_EMAILTEMPLATE_DETAIL = "Cannot get details of the \"{0}\" email template.";
        public const string ERROR_EMAILTEMPLATE_CREATE_UNEXPECTED = "Unexpected error when creating the \"{0}\" email template.";
        public const string ERROR_EMAILTEMPLATE_UPDATE_UNEXPECTED = "Unexpected error when updating the \"{0}\" email template.";

        public const string ERROR_EMAILSERVERCONFIG = "Cannot configure email server info.";
        
        public const string ERROR_PUBLISHCONFIG = "Cannot configure publishing info.";

        public const string ERROR_CREDENTIALS_INCORRECT = "The provided credentials info is incorrect.";
        
        public const string ACTION_EVENT_CREATE_CONFLICT = "\"{0}\" is already existed.";
    }
}