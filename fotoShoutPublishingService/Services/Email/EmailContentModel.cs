using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FotoShoutPublishingService.Services.Email {
    internal class EmailContentModel {
        internal string Name { get; set; }
        internal string FirstName { get; set; }
        internal string LastName { get; set; }
        internal string Email { get; set; }
        internal string PermaLink { get; set; }
    }
}
