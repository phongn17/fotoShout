using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class EventBroadcast {
        public int EventBroadcastId { get; set; }
        public int BroadcastId { get; set; }
        public Guid PhotoId { get; set; }
        public int EventId { get; set; }
        public byte Status { get; set; }
        public string Thumbnails { get; set; }
        public string PermaLinks { get; set; }
        public string Error { get; set; }
    }
}