using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maintainify.Core.Entity.ApplicationData
{
    public class Images
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Image Name")]
        public string ImageName { get; set; }

        [ForeignKey("Path")]
        [Display(Name = "Image Path")]
        public string PathId { get; set; }

        public PathFiles pathFiles { get; set; }
    }
}
