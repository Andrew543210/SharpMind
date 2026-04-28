using System.ComponentModel.DataAnnotations;

namespace SharpMind.ViewModels.Account;

public class RegisterViewModel
{
    [Required, StringLength(60)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

