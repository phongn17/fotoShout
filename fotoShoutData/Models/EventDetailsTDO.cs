using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models {
    public class EventDetailsTDO {
        public EventOption EventOption { get; set; }
        public HashSet<dynamic> PhotosDetails { get; set; }
    }
}
