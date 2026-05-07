namespace URLShortener.Application.Configuration;

public sealed record IpInfoSettings
{
    public string AccessToken { get; init; } = string.Empty;
}
