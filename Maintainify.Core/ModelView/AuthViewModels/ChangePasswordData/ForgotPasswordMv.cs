using System.ComponentModel.DataAnnotations;


namespace Maintainify.Core.ModelView.AuthViewModel.ChangePasswordData;

public class ForgotPasswordMv
{
    [Required(ErrorMessage = "يجب أدخال رقم الهاتف ")]
    [Display(Name = "رقم الهاتف")]
    [RegularExpression("^(00|0|)?(966|5|)(\\d{9})$", ErrorMessage = " Please enter your correct phone number  ")]
    public string PhoneNumber { get; set; }
}