using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class EmailTemplate {
        public int EmailTemplateId { get; set; }
        
        [Required]
        [Display(Name = "Template Name")]
        public string EmailTemplateName { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public string EmailSubject { get; set; }

        [Required]
        [Display(Name = "Content")]
        public string EmailContent { get; set; }

        public virtual User User { get; set; }
    }
}