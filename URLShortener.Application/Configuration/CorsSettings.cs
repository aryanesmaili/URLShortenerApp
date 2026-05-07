namespace URLShortener.Application.Configuration;

public sealed record CorsSettings
{
    public string[] AllowedOrigins { get; init; } = [];

    public string[] AllowedMethods { get; init; } = [];

    public string[] AllowedHeaders { get; init; } = [];

    public bool AllowCredentials { get; init; } = true;
}
