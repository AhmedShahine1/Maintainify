using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Maintainify.Core.ModelView.AuthViewModel.UpdateData;

public class UpdateUserMv
{
    [Required, StringLength(50), MinLength(1)]
    public string FirstName { get; set; }

    [Required, StringLength(50), MinLength(1)]
    public string LastName { get; set; }

    [Required, StringLength(128)]
    [DataType(DataType.EmailAddress)]
    [EmailAddress(ErrorMessage = "البريد الالكتروني غير صحيح")]
    public string Email { get; set; }

    [Display(Name = "رقم الهاتف")]
    [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
    [Required]
    public string PhoneNumber { get; set; }

    [Required, StringLength(int.MaxValue)]
    [Display(Name = "المؤهل")]
    public string Qualification { get; set; }

    [Required]
    [Display(Name = "العمر")]
    public int age { get; set; }

    [Required, StringLength(int.MaxValue)]
    [Display(Name = "الوظيفه")]
    public string Job { get; set; }

    [Required, StringLength(256)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required, StringLength(256)]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    [Required]
    public IFormFile Img { get; set; } // user Img base64


}