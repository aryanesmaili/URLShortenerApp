namespace URLShortener.Application.DTOs.Settings;

public sealed record RedisConnectionCreds
{
    public required string Host { get; init; }
    public int Port { get; init; }
}

