using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.Settings;

public sealed record AntiforgerySettings
{
    [Required]
    public string HeaderName { get; init; } = "X-XSRF-TOKEN";

    [Required]
    public string CookieName { get; init; } = "XSRF-TOKEN";

    [Required]
    public string CookieSameSite { get; init; } = "Strict";

    [Required]
    public string CookieSecurePolicy { get; init; } = "Always";

    [Required]
    public string CookiePath { get; init; } = "/";

    public bool CookieHttpOnly { get; init; }
}
