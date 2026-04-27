using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.Settings;

public sealed record ApplicationInfoSettings
{
    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    [Required]
    public string EmailChangeConfirmationPath { get; init; } = "/confirm-email-change";

    [Required]
    public string PasswordResetPath { get; init; } = "/ResetPassword";
}
