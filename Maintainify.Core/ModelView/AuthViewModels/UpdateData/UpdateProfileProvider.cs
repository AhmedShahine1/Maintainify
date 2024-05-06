using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintainify.Core.ModelView.AuthViewModels.UpdateData
{
    public class UpdateProfileProvider
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

        [StringLength(128)]
        [Display(Name = "المهنه")]
        public string ProfessionId { get; set; }

        [StringLength(int.MaxValue), MinLength(1)]
        public string Description { get; set; }

        [StringLength(int.MaxValue), MinLength(1)]
        public string bankAccountNumber { get; set; }

        public List<IFormFile> PreviousWork { get; set; }

    }
}
