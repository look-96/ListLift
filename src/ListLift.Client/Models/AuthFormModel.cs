using System.ComponentModel.DataAnnotations;

namespace ListLift.Client.Models;

public sealed class AuthFormModel
{
    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(256)]
    public string Password { get; set; } = string.Empty;
}
