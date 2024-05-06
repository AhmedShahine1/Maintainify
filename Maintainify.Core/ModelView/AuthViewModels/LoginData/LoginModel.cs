using System.ComponentModel.DataAnnotations;

namespace Maintainify.Core.ModelView.AuthViewModel.LoginData;

public class LoginModel
{

    [Required(ErrorMessage = "يجب أدخال اسم المستخدم ")]
    [Display(Name = "Name")]
    public string Name { get; set; }

    [Required(ErrorMessage = "يجب أدخال كلمة السر ")]
    [Display(Name = "Password")]
    public string Password { get; set; }

    public bool IsPersist { get; set; }
}