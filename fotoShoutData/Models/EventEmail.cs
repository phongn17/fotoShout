using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models {
    public partial class EventEmail {
        public int EventEmailId { get; set; }
        public Guid GuestPhotoId { get; set; }
        public int EventId { get; set; }
        public byte Status { get; set; }
        public string Error { get; set; }
    }
}
