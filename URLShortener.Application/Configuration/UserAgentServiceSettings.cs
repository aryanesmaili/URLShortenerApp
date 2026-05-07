using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.Configuration;

public sealed record UserAgentServiceSettings
{
    [Required]
    public string ApiAddress { get; init; } = string.Empty;
}
