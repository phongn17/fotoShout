using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models.Publish {
    public class Channel {
        public int ID { get; set; }
        public int WSAID { get; set; }
        public string Name { get; set; }
        public string PermaLink { get; set; }
        public string Description { get; set; }
        public string AccountURL { get; set; }
        public bool IsAuthorized { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public Guid? ChannelKey { get; set; }
        public string AccountName { get; set; }
        public string ThumbnailURL { get; set; }
        public string Status { get; set; }
        public DateTime? LastJobRun { get; set; }
        public List<ChannelAction> Actions { get; set; }
        public List<Guid> GroupACL { get; set; }
        public List<int> UserACL { get; set; }
    }
}
