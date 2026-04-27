using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.Settings;

public sealed record RedisCacheSettings
{
    [Required]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; } = 6379;

    public string Password { get; init; } = string.Empty;

    public string InstanceName { get; init; } = string.Empty;
}
