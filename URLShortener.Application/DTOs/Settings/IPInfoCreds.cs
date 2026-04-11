namespace URLShortener.Application.DTOs.Settings;

public sealed record IPInfoCreds
{
    public required string AccessToken { get; init; }
}
