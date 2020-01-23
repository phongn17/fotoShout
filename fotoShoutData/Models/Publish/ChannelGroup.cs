using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models.Publish {
    public class ChannelGroup {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public Guid? TemplateKey { get; set; }
        public List<Channel> Channels { get; set; }
        public List<ChannelGroupAction> Actions { get; set; }
        public List<BroadcastField> Fields { get; set; }
    }
}
