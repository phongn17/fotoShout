using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class PagingInfo {
        public int TotalPhotos { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }

        public int TotalPages {
            get {
                return (int)Math.Ceiling((decimal)TotalPhotos / PageSize);
            }
        }
    }
}