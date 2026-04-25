namespace URLShortener.Application.DTOs.Settings;

public sealed record JwtSettings
{
    public string? TokenSecretKey { get; init; }
    public required string RefreshHashSalt { get; init; }
    public string? Issuer { get; init; }
    public string? Audience { get; init; }
    public int ExpiresInMinutes { get; init; }
}
