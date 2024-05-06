using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintainify.Core.ModelView.AuthViewModels.UpdateData
{
    public class UpdateProfileSeeker
    {
        [Required, StringLength(50), MinLength(1)]
        public string FullName { get; set; }

        [Required, StringLength(128)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "البريد الالكتروني غير صحيح")]
        public string Email { get; set; }

        [Display(Name = "رقم الهاتف")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [Required]
        public string PhoneNumber { get; set; }

        public float? Lat { get; set; }

        public float? Lng { get; set; }

    }
}
