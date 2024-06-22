//using Maintainify.Core.Entity.ApplicationData;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Maintainify.Core.Entity.EvaluationData
//{
//    public class Evaluation : BaseEntity
//    {
//        [ForeignKey("Seeker")]
//        [Display(Name = "اسم العميل ")]
//        [Required]
//        public string SeekerId { get; set; }
//        [Display(Name = "اسم العميل ")]
//        public virtual ApplicationUser Seeker { get; set; }
//    }
//}
