using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutApi {
    internal class Errors {
        internal const string ERROR_AUTHORIZATIONKEY_EMPTY = "API is secure. You need an API key prior to use it.";
        internal const string ERROR_AUTHORIZATIONKEY_INVALID = "Authorization key is invalid.";

        internal const string ERROR_CREDENTIALS_EMPTY = "Credentials is empty.";
        internal const string ERROR_APIKEY_NOTFOUND = "API key not found.";
        internal const string ERROR_LOGIN_INVALID = "Email/password is invalid.";
        internal const string ERROR_LOGIN_NOACCOUNT = "Email and password are not associated with an API key.";

        internal const string ERROR_EVENT_NOTFOUND = "An event with the id {0} not found for the current user.";
        internal const string ERROR_EVENT_DELETE_ANNOTATED = "The \"{0}\" event has been annotated and cannot be deleted.";
        internal const string ERROR_EVENT_CLEARREL = "Unexpected error: cannot delete the \"{0}\" event.";

        internal const string ERROR_BROADCAST_UPDATE_IDSNOTIDENTICAL = "Ids need to be identical when updating a brocast [{0}, {1}].";

        internal const string ERROR_PUBLISHACCOUNT_UPDATE_IDSNOTIDENTICAL = "Ids need to be identical when updating a publish account [{0}, {1}].";
        
        internal const string ERROR_EMAILSERVERCONFIG_UPDATE_IDSNOTIDENTICAL = "Ids need to be identical when updating an email server account [{0}, {1}].";

        internal const string ERROR_EVENTPUBLISH_UPDATE_IDSNOTIDENTICAL = "Ids need to be identical when updating an event channel group [{0}, {1}].";

        internal const string ERROR_NOACCOUNT = "Unexpected error: No account info.";
    }
}