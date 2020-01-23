using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public enum EventBroadcastStatus {
        Pending = 0,
        Processed = 1,
        PublishPending = 2,
        Error = 255
    }
}