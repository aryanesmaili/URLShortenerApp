using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed class UserCreateDTO
{
    private string? _name;

    [Required(ErrorMessage = "Full Name is Required.")]
    [StringLength(64, MinimumLength = 5, ErrorMessage = "Your Full Name should have at least 5 Characters and at most 64 Characters.")]
    public required string Name
    {
        get => _name ?? string.Empty;
        set => _name = value?.Trim();
    }

    private string? _email;

    [Required(ErrorMessage = "Email is Required.")]
    [EmailAddress(ErrorMessage = "Entered value is not a valid Email.")]
    public required string Email
    {
        get => _email ?? string.Empty;
        set => _email = value?.Trim();
    }

    private string? _username;

    [Required(ErrorMessage = "Username is Required.")]
    [StringLength(32, MinimumLength = 5, ErrorMessage = "Your Username should have at least 5 Characters and at most 32 Characters.")]
    public required string Username
    {
        get => _username ?? string.Empty;
        set => _username = value?.Trim();
    }

    private string? _password;

    [Required(ErrorMessage = "Password is Required.")]
    [StringLength(64, MinimumLength = 5, ErrorMessage = "Your Password should have at least 5 Characters and at most 64 Characters.")]
    public required string Password
    {
        get => _password ?? string.Empty;
        set => _password = value?.Trim();
    }

    private string? _confirmPassword;

    [Required(ErrorMessage = "Confirm Password is Required.")]
    [StringLength(64, MinimumLength = 5, ErrorMessage = "Your Confirm Password should have at least 5 Characters and at most 64 Characters.")]
    [Compare("Password", ErrorMessage = "Your Password is not equal to your Confirm Password.")]
    public required string ConfirmPassword
    {
        get => _confirmPassword ?? string.Empty;
        set => _confirmPassword = value?.Trim();
    }
}

