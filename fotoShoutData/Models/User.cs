using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class User {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public char MiddleInitial { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public string PhoneExt { get; set; }
        [Required]
        public string Email { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }
        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
        public int CreatedBy { get; set; }
        public DateTime Created { get; set; }

        [Required]
        public virtual UserRole Role { get; set; }
        public virtual UserAuthorization Authorization { get; set; }

        [Required]
        public virtual Account Account { get; set; }
    }
}