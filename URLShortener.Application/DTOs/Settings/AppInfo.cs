namespace URLShortener.Application.DTOs.Settings;

public sealed record AppInfo
{
    public required string BaseURL { get; init; }
}
