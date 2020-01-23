using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models
{
    public class PhotoEmail {
        public int PhotoEmailId { get; set; }

        public Guid PhotoId { get; set; }

        public Guid GuestId { get; set; }

        public int EventId { get; set; }

        public byte Status { get; set; }

        public string Error { get; set; }
    }
}
