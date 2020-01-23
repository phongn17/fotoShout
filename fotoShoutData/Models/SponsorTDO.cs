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
        public string SponsorHeaderImage { get; set; }
        [Display(Name = "Header Image")]
        public HttpPostedFileBase SponsorHeaderFile { get; set; }
        [Display(Name = "Header URL")]
        public string SponsorHeaderUrl { get; set; }
        public string SponsorFooterImage { get; set; }
        [Display(Name = "Footer Image")]
        public HttpPostedFileBase SponsorFooterFile { get; set; }
        [Display(Name = "Footer URL")]
        public string SponsorFooterUrl { get; set; }
        public string SponsorTopInfoBlockImage { get; set; }
        [Display(Name = "Top Block Image")]
        public HttpPostedFileBase SponsorTopInfoBlockFile { get; set; }
        [Display(Name = "Top Block URL")]
        public string SponsorTopInfoBlockUrl { get; set; }
        public string SponsorBottomInfoBlockImage { get; set; }
        [Display(Name = "Bottom Block Image")]
        public HttpPostedFileBase SponsorBottomInfoBlockFile { get; set; }
        [Display(Name = "Bottom Block URL")]
        public string SponsorBottomInfoBlockUrl { get; set; }
    }
}