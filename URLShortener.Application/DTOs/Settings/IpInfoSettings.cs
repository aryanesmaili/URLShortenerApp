namespace URLShortener.Application.DTOs.Settings;

public sealed record IpInfoSettings
{
    public string AccessToken { get; init; } = string.Empty;
}
