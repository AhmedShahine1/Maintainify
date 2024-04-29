using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintainify.Core.ModelView.AuthViewModels.RegisterData
{
    public class RegisterSeeker
    {
        [Required, StringLength(100), MinLength(1)]
        public string FullName { get; set; }

        [Required, StringLength(128)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "البريد الالكتروني غير صحيح")]
        public string Email { get; set; }

        [Display(Name = "رقم الهاتف")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [Required]
        public string PhoneNumber { get; set; }

        [Required, StringLength(256)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
