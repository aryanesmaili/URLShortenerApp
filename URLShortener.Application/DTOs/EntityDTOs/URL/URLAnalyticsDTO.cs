namespace URLShortener.Application.DTOs.EntityDTOs.URL;

public sealed record URLAnalyticsDTO
{
    public long ID { get; init; }
    private string mostUsedLocationsJSON = string.Empty;

    public required string MostUsedLocationsJSON
    {
        get => mostUsedLocationsJSON;
        init => mostUsedLocationsJSON = value.Trim();
    }

    private string mostUsedDevicesJSON = string.Empty;
    public required string MostUsedDevicesJSON
    {
        get => mostUsedDevicesJSON;
        init => mostUsedDevicesJSON = value.Trim();
    }

    public int ClickCount { get; init; } = 0;
    public DateTime LastTimeCalculated { get; init; }

    public long URLID { get; init; }
}
