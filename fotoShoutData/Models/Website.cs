using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class Website: WebsiteInfo {
        public Website() {
            this.Events = new HashSet<Event>();
        }

        public int WebsiteId { get; set; }

        [Required]
        public string WebsiteName { get; set; }

        public ICollection<Event> Events { get; set; }
        public virtual User User { get; set; }
    }
}