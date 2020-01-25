using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models {
    public class PhotoGuest {
        public Guid PhotoId { get; set; }
        public Guid GuestId { get; set; }
    }
}
