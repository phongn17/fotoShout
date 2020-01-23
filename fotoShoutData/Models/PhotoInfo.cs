using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models {
    public class PhotoInfo {
        public Guid PhotoId { get; set; }
        public string Folder { get; set; }
        [Required]
        public string Filename { get; set; }
        public string Image { get; set; }
        public byte Status { get; set; }
        public string Thumbnail { get; set; }
        public System.DateTime Created { get; set; }
        public int SubmittedBy { get; set; }
        public System.DateTime? Submitted { get; set; }
        [Range(0, 5)]
        public int Rating { get; set; }
    }
}
