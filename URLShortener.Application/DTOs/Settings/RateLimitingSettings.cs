namespace URLShortener.Application.DTOs.Settings;

public sealed record RateLimitingSettings
{
    public int RejectionStatusCode { get; init; } = 429;

    public Dictionary<string, RateLimitPolicySettings> Policies { get; init; } = new(StringComparer.Ordinal);
}

public sealed record RateLimitPolicySettings
{
    public string Algorithm { get; init; } = "SlidingWindow";

    public int PermitLimit { get; init; } = 1;

    public int WindowSeconds { get; init; } = 60;

    public int SegmentsPerWindow { get; init; } = 4;

    public string PartitionStrategy { get; init; } = "Global";
}
