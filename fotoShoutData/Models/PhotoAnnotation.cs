using FotoShoutData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class PhotoAnnotation {
        public int Rating { get; set; }
        public ICollection<GuestTDO> Guests { get; set; }
    }
}