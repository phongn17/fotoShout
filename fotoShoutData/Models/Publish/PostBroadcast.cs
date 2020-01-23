using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models.Publish {
    public class PostBroadcast {
        public int ID { get; set; }
        public int TemplateID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string StartTime { get; set; }
        public int? AssignedTo { get; set; }
        public int? AssignedBy { get; set; }
        public string AssignedDate { get; set; }
        public string ScheduledTime { get; set; }
        public string DueDate { get; set; }
        public List<BroadcastFieldValue> BroadcastFields { get; set; }
    }
}
