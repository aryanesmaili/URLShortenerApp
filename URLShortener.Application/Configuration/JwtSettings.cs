using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.Configuration;

public sealed record JwtSettings
{
    [Required]
    public string TokenSecretKey { get; init; } = string.Empty;

    [Required]
    public string RefreshHashSalt { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Range(1, 525600)]
    public int ExpiresInMinutes { get; init; } = 30;
}
