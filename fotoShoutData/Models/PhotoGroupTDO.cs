using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FotoShoutData.Models {
    public class PhotoGroupTDO {
        public string Thumbnail { get; set; }
        public DateTime Created { get; set; }
        public int NumPhotos { get; set; }
    }
}
