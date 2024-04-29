using System.ComponentModel.DataAnnotations;

namespace Maintainify.Core.ModelView.AuthViewModel.RoleData;

public class AddRoleModel
{
    [Required]
    public string UserId { get; set; }

    [Required]
    public List<string> Roles { get; set; } = new();
}