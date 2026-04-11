namespace URLShortener.Application.DTOs.Settings;

public sealed class JwtSettings
{
    public string? SecretKey { get; init; }
    public string? Issuer { get; init; }
    public string? Audience { get; init; }
    public int ExpiresInMinutes { get; init; }
}
