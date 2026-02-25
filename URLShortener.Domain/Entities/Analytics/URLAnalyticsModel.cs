using URLShortener.Domain.Entities.URL;

namespace URLShortener.Domain.Entities.Analytics;

public class URLAnalyticsModel
{
    public int ID { get; set; }

    public required string MostUsedLocationsJSON { get; set; }
    public required string MostUsedDevicesJSON { get; set; }

    public int ClickCount { get; set; } = 0;
    public DateTime LastTimeCalculated { get; set; }

    public int URLID { get; set; }
    public required URLModel URL { get; set; }
}