using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    /// <summary>
    /// Used as a photo model to transfer to views
    /// </summary>
    public class PhotoTDO: PhotoInfo {
        public virtual IEnumerable<GuestTDO> Guests { get; set; }
    }
}