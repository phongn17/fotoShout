using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models.Publish {
    public class ChannelGroupAction {
        public int ChannelID { get; set; }
        public int PublishActionID { get; set; }
        public bool? IsPrimary { get; set; }
    }
}
