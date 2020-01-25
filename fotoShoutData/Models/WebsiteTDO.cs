using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class WebsiteTDO {
        public int WebsiteId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string WebsiteName { get; set; }
        public string HeaderImage { get; set; }
        [Display(Name = "Header Image")]
        public HttpPostedFileBase HeaderFile { get; set; }
        [Display(Name = "Header URL")]
        public string HeaderUrl { get; set; }
        public string FooterImage { get; set; }
        [Display(Name = "Footer Image")]
        public HttpPostedFileBase FooterFile { get; set; }
        [Display(Name = "Footer URL")]
        public string FooterUrl { get; set; }
        public string TopInfoBlockImage { get; set; }
        [Display(Name = "Top Block Image")]
        public HttpPostedFileBase TopInfoBlockFile { get; set; }
        [Display(Name = "Top Block URL")]
        public string TopInfoBlockUrl { get; set; }
        public string BottomInfoBlockImage { get; set; }
        [Display(Name = "Bottom Block Image")]
        public HttpPostedFileBase BottomInfoBlockFile { get; set; }
        [Display(Name = "Bottom Block URL")]
        public string BottomInfoBlockUrl { get; set; }
    }
}