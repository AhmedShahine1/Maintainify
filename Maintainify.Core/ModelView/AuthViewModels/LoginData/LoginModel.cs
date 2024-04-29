using System.ComponentModel.DataAnnotations;

namespace Maintainify.Core.ModelView.AuthViewModel.LoginData;

public class LoginModel
{

    [Required(ErrorMessage = "يجب أدخال الايميل ")]
    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "الايميل غير صحيح")]
    public string Email { get; set; }

    [Required(ErrorMessage = "يجب أدخال كلمة السر ")]
    [Display(Name = "Password")]
    public string Password { get; set; }

    public bool IsPersist { get; set; }
}