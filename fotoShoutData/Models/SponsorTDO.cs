using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class SponsorTDO {
        public int SponsorId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string SponsorName { get; set; }
        [Display(Name = "Logo URL")]
        public string SponsorLogo { get; set; }
    }
}