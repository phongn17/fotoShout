﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class Sponsor {
        public Sponsor() {
            this.Events = new HashSet<Event>();
        }

        public int SponsorId { get; set; }
        
        [Required]
        public string SponsorName { get; set; }

        public string SponsorLogo { get; set; }
        public virtual ICollection<Event> Events { get; set; }
        public virtual User User { get; set; }
    }
}