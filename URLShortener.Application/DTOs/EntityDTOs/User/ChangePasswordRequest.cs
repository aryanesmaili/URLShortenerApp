using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed class ChangePasswordRequest
{
    [Required(ErrorMessage = "A valid UserInfo is required.")]
    public required UserDTO UserInfo { get; set; }

    private string? _newPassword;

    [Required(ErrorMessage = "Password is Required.")]
    [StringLength(64, MinimumLength = 5, ErrorMessage = "Your Password should have at least 5 Characters and at most 64 Characters.")]
    public required string NewPassword
    {
        get => _newPassword ?? string.Empty;
        set => _newPassword = value?.Trim();
    }

    private string? _confirmPassword;

    [Required(ErrorMessage = "Confirm Password is Required.")]
    [StringLength(64, MinimumLength = 5, ErrorMessage = "Your Confirm Password should have at least 5 Characters and at most 64 Characters.")]
    [Compare("NewPassword", ErrorMessage = "Your Password is not equal to your Confirm Password.")]
    public required string ConfirmPassword
    {
        get => _confirmPassword ?? string.Empty;
        set => _confirmPassword = value?.Trim();
    }
}

