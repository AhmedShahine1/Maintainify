using Maintainify.Core.Entity.ApplicationData;
//using Maintainify.Core.Entity.EvaulationData;
using Maintainify.Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintainify.Core.Entity.OrderData
{
    public class Order : BaseEntity
    {
        [ForeignKey("Provider")]
        [Required(ErrorMessage = "Required Choose Provider")]
        public string ProviderId { get; set; }
        public virtual ApplicationUser Provider { get; set; }

        [ForeignKey("Seeker")]
        [Required(ErrorMessage = "Required Choose Seeker")]
        public string SeekerId { get; set; }
        public virtual ApplicationUser Seeker { get; set; }

        [Display(Name ="رقم الطلب")] 
        public string numberOrder {  get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Display(Name = "حالة الطلب")]
        public OrderStatus OrderStatus { get; set; }

        //------------------------------------------------------------------------------------
        //public EvaluationOrder evaluationOrder { get; set; }
    }
}
