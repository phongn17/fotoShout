using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class Account {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public Guid ApiKey { get; set; }
        public string Notes { get; set; }
    }
}