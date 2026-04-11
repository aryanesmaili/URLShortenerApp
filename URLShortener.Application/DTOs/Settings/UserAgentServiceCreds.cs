namespace URLShortener.Application.DTOs.Settings;

public sealed record UserAgentServiceCreds
{
    public required string APIAddress { get; init; }
}
