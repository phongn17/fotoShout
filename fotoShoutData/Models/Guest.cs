using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class Guest: GuestInfo {
        public Guest() {
            this.GuestPhotos = new HashSet<GuestPhoto>();
        }
    
        public virtual ICollection<GuestPhoto> GuestPhotos { get; set; }
    }
}