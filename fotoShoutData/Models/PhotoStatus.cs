using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public enum PhotoStatus {
        Unselected = 0,
        Selected = 1,
        Submitted = 2,
        PendingPublish = 3,
        Published = 4,
        Unclaimed = 5
    }
}