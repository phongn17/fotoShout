using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models {
    public class ChannelGroupTDO {
        public int ChannelGroupId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string ChannelGroupName { get; set; }
    }
}
