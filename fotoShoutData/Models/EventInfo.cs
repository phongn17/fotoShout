using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models {
    public class EventInfo {
        public int EventId { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string EventName { get; set; }
        [Display(Name = "Description")]
        public string EventDescription { get; set; }
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime EventDate { get; set; }
        [Display(Name = "Location")]
        public string EventLocation { get; set; }
        [Required]
        [Display(Name = "Folder")]
        public string EventFolder { get; set; }
        public string EventVirtualPath { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime? Created { get; set; }
        public byte EventStatus { get; set; }
        [Display(Name = "Album Name")]
        public string PublishAlbum { get; set; }
    }
}
