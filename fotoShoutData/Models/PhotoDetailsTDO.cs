using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models {
    public class PhotoDetailsTDO: PhotoTDO {
        public string Thumbnails { get; set; }
        public string PermaLinks { get; set; }
        public string Error { get; set; }
    }
}
