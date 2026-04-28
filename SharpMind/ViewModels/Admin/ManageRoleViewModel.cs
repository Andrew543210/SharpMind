using System.ComponentModel.DataAnnotations;
using SharpMind.Models.Identity;

namespace SharpMind.ViewModels.Admin;

public class ManageRoleViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SelectedRole { get; set; } = AppRoles.Student;

    public IReadOnlyList<string> Roles { get; set; } = AppRoles.All;
}

