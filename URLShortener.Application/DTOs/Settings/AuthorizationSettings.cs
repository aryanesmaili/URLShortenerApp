using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.Settings;

public sealed record AuthorizationSettings
{
    public Dictionary<string, string[]> Policies { get; init; } = new(StringComparer.Ordinal);

    public Dictionary<string, string> Roles { get; init; } = new(StringComparer.Ordinal);

    [Required]
    public string DefaultRegistrationRole { get; init; } = "User";
}
