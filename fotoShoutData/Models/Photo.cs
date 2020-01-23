using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models
{
    public class Photo: PhotoInfo
    {
        public Photo()
        {
            this.GuestPhotos = new HashSet<GuestPhoto>();
        }

        public string Error { get; set; }

        public virtual ICollection<GuestPhoto> GuestPhotos { get; set; }
        public virtual Event Event { get; set; }
    }
}