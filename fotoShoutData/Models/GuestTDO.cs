using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class GuestTDO: GuestInfo {
        public bool AuthorizePublish { get; set; }
    }
}