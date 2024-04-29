using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace Maintainify.Core.ModelView.AuthViewModel.ChangePasswordData;

public class ChangePasswordMv
{
    [Display(Name = "Password")]
    [Required(ErrorMessage = "كلمة السر مطلوبة ")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Confirm Password")]
    [Required(ErrorMessage = "تأكيد كلمة السر مطلوب ")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "كلمة المرور وكلمة المرور التأكيدية غير متطابقتين.")]
    public string ConfirmPassword { get; set; }

       
    [JsonIgnore]
    public string UserId { get; set; }
}