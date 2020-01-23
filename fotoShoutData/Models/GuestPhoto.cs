using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class GuestPhoto {
        public GuestPhoto() {
            AuthorizePublish = false;
        }

        public Guid Id { get; set; }
        
        public int Event_EventId { get; set; }
        public bool AuthorizePublish { get; set; }

        public virtual Guest Guest { get; set; }
        public virtual Photo Photo { get; set; }
    }
}