using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FotoShoutData.Models
{
    public class Event: EventInfo
    {
        public Event()
        {
            this.Photos = new HashSet<Photo>();
            this.Sponsors = new HashSet<Sponsor>();
            this.Guests = new HashSet<Guest>();
        }

        public ICollection<Photo> Photos { get; set; }
        public ICollection<Sponsor> Sponsors { get; set; }
        public ICollection<Guest> Guests { get; set; }
        public int? ChannelGroupId { get; set; }
        [Required]
        public EventOption EventOption { get; set; }
        public EmailTemplate EmailTemplate { get; set; }
        public User User { get; set; }
    }
}