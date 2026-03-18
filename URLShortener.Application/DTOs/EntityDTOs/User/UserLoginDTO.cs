using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed class UserLoginDTO
{
    private string? _identifier;

    [Required(ErrorMessage = "Email or Username is Required.")]
    public string? Identifier
    {
        get => _identifier;
        set => _identifier = value?.Trim();
    }

    private string? _password;

    [Required(ErrorMessage = "Password is Required.")]
    [StringLength(64, MinimumLength = 5, ErrorMessage = "Your Password should have at least 5 Characters and at most 64 Characters.")]
    public string? Password
    {
        get => _password;
        set => _password = value?.Trim();
    }
}

