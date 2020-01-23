using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models {
    public class EmailServerAccount {
        public int EmailServerAccountId { get; set; }

        [Required]
        [Display(Name = "Server")]
        public string Server { get; set; }

        [Display(Name = "Server Port")]
        public int? Port { get; set; }

        [Display(Name = "Domain")]
        public string Domain { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Enable SSL")]
        public bool EnableSSL { get; set; }
        
        public virtual User User { get; set; }
    }
}
