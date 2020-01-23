using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models {
    public class EventOptionInfo {
        public int EventOptionId { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string EventOptionName { get; set; }

        [Display(Name = "FirstName")]
        public bool FirstNameOption { get; set; }

        [Display(Name = "LastName")]
        public bool LastNameOption { get; set; }

        [Display(Name = "Email")]
        public bool EmailOption { get; set; }

        [Display(Name = "Phone")]
        public bool PhoneOption { get; set; }

        [Display(Name = "Fax")]
        public bool FaxOption { get; set; }

        [Display(Name = "Address")]
        public bool AddressOption { get; set; }

        [Display(Name = "Authorize to publish & Signature")]
        public bool SignatureOption { get; set; }

        [Display(Name = "Salutation")]
        public bool SalutationOption { get; set; }
    }
}
