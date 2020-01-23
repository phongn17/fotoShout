using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models.Publish {
    public class ChannelAction {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? IsDefault { get; set; }
        public bool? ReturnsPermalink { get; set; }
        public bool? ReturnsEmbedCode { get; set; }
    }
}
