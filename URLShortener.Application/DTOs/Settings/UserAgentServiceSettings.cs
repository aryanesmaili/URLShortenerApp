using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.Settings;

public sealed record UserAgentServiceSettings
{
    [Required]
    public string ApiAddress { get; init; } = string.Empty;
}
