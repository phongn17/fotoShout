using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models {
    public class PhotoAuthorizationTDO {
        public Guid PhotoId { get; set; }
        public string Filename { get; set; }
        public IEnumerable<GuestTDO> Guests { get; set; }
    }
}
