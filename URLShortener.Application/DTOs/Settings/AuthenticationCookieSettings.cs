using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.Settings;

public sealed record AuthenticationCookieSettings
{
    [Required]
    public string JwtCookieName { get; init; } = "jwt";

    [Required]
    public string RefreshTokenCookieName { get; init; } = "refreshToken";

    [Required]
    public string Path { get; init; } = "/";

    public bool HttpOnly { get; init; } = true;

    public bool Secure { get; init; } = true;

    [Required]
    public string JwtSameSite { get; init; } = "Strict";

    [Required]
    public string RefreshTokenSameSite { get; init; } = "Lax";

    [Range(1, 3650)]
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
