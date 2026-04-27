using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.Settings;

public sealed record SmtpSettings
{
    [Required]
    public string Server { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; } = 587;

    [Required]
    public string SenderName { get; init; } = string.Empty;

    [Required]
    public string SenderEmail { get; init; } = string.Empty;

    [Required]
    public string Username { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    public bool EnableSsl { get; init; } = true;
}
