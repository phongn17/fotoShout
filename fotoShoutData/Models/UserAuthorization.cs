using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class UserAuthorization {
        public int Id { get; set; }
        [Required]
        public Guid AuthorizationKey { get; set; }
        public DateTime Created { get; set; }
    }
}