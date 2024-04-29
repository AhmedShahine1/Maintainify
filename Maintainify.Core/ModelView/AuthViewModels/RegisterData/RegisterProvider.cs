using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Maintainify.Core.ModelView.AuthViewModel.RegisterData
{
    public class RegisterProvider
    {
        [Required, StringLength(100), MinLength(1)]
        public string FullName { get; set; }

        [Required, StringLength(128)]
        [Display(Name = "المهنه")]
        public string ProfessionId { get; set; }

        [Required, StringLength(128)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "البريد الالكتروني غير صحيح")]
        public string Email { get; set; }

        [Display(Name = "رقم الهاتف")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        [Required]
        public string PhoneNumber { get; set; }

        [Required, StringLength(int.MaxValue)]
        [Display(Name = "الوصف")]
        public string Description { get; set; }

        [Required, StringLength(int.MaxValue)]
        [Display(Name = "رقم الحساب البنكي")]
        public string bankAccountNumber { get; set; }

        [Required, StringLength(256)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
