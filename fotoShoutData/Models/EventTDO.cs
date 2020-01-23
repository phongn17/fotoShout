using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models {
    public class EventTDO: EventInfo {
        [Display(Name = "Thumbnail")]
        public string Thumbnail { get; set; }

        [Display(Name = "Option")]
        public int EventOptionId { get; set; }
        [Display(Name = "Email Template")]
        public int? EmailTemplateId { get; set; }
        [Display(Name = "Sponsors")]
        public IEnumerable<int> SponsorIds { get; set; }
        [Display(Name = "Channel Group")]
        public int? ChannelGroupId { get; set; }

        public IEnumerable<EventOptionTDO> EventOptions { get; set; }
        public IEnumerable<SponsorTDO> Sponsors { get; set; }
        public IEnumerable<EmailTemplateTDO> EmailTemplates { get; set; }
        public IEnumerable<ChannelGroupTDO> ChannelGroups { get; set; }
    }
}