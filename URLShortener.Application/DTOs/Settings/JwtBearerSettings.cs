namespace URLShortener.Application.DTOs.Settings;

public sealed record JwtBearerSettings
{
    public bool RequireHttpsMetadata { get; init; } = true;

    public bool SaveToken { get; init; } = true;
}
