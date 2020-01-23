using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class PublishAccount {
        public int Id { get; set; }

        [Required]
        [Display(Name = "URL")]
        public string Url { get; set; }

        [Display(Name = "API Key")]
        public Guid ApiKey { get; set; }
        
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public virtual Account Account { get; set; }
    }
}