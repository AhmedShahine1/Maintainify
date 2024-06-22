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
//    public class EvaluationProvider : Evaluation
//    {
//        [ForeignKey("Provider")]
//        [Display(Name = "اسم مقدم الخدمه ")]
//        [Required]
//        public string ProviderId { get; set; }
//        [Display(Name = "اسم مقدم الخدمه ")]
//        public virtual ApplicationUser Provider { get; set; }


//        [Display(Name ="اعجبني")]
//        public int Likes { get; set; }
//    }
//}
