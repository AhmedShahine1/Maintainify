using Maintainify.Core.Entity.ApplicationData;
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
        [Required, StringLength(int.MaxValue,ErrorMessage ="Required Choose Provider")]
        [ForeignKey(nameof(Provider))]
        public string ProviderId { get; set; }
        public ApplicationUser Provider { get; set; }
        [Required, StringLength(int.MaxValue, ErrorMessage = "Required Choose Seeker")]
        [ForeignKey(nameof(Seeker))]
        public string SeekerId { get; set; }
        public ApplicationUser Seeker { get; set; }
    }
}
