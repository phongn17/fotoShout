using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models {
    public class EmailTemplateTDO {
        public int EmailTemplateId { get; set; }

        [Required]
        [Display(Name = "Template Name")]
        public string EmailTemplateName { get; set; }
    }
}
