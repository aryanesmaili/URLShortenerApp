using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.Settings;

public sealed record SignalRSettings
{
    [Required]
    public string UserHubPath { get; init; } = "api/User/UserHub";

    public bool RequireAuthorization { get; init; } = true;

    public string AuthorizationPolicyName { get; init; } = "AllUsers";
}
