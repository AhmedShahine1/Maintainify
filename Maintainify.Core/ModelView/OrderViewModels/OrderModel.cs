using Maintainify.Core.Entity;
using Maintainify.Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Maintainify.Core.ModelView.OrderViewModels
{
    public class OrderModel : BaseEntity
    {
        [Required(ErrorMessage = "يجب اختيار مقدم خدمه"), NotNull, Display(Name = "مقدم الخدمه")]
        public string ProviderId { get; set; }

        public string? SeekerId { get; set; }

        [Required]
        [ValidDateTime]
        public string OrderDate { get; set; }

        public string? Status { get; set; } = "Preparing";

    }
}
