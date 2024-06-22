//using Maintainify.Core.Entity.ApplicationData;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Maintainify.Core.Entity.OrderData;
//using Maintainify.Core.Entity.EvaluationData;

//namespace Maintainify.Core.Entity.EvaulationData
//{
//    public class EvaluationOrder : Evaluation
//    {
//        [Required(ErrorMessage = "التقييم مطلوب ")]
//        [Display(Name = "التقييم ")]
//        [Range(1, 5, ErrorMessage = "التقييم يجب ان يكون بين 1 و 5 ")]
//        public int NumberOfStars { get; set; }

//        [Display(Name ="قيمه المبلغ المدفوع في الخدمه")]
//        public int Price { get; set; } = 0;

//        [Required, Display(Name ="هل انت راض عن الخدمه")]
//        public bool satisfied { get; set; }

//        [ForeignKey("order")]
//        [Display(Name ="الخدمه")]
//        [Required]
//        public string OrderId { get; set; }
//        [Display(Name = "الخدمه")]
//        public Order order { get; set; }

//    }
//}
