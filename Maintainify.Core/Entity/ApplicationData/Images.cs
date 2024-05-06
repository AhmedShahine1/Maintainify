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

        [ForeignKey("pathFiles")]
        [Display(Name = "Image Path")]
        public string PathId { get; set; }

        public PathFiles pathFiles { get; set; }

        //-----------------------------------------

        [ForeignKey("User")]
        [Display(Name = "المستخدم ")]
        public string UserId { get; set; }
        [Display(Name = "اسم المستخدم ")]
        public virtual ApplicationUser User { get; set; }

    }
}
