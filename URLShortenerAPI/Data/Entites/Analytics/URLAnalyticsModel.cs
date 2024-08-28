using URLShortenerAPI.Data.Entites.URL;

namespace URLShortenerAPI.Data.Entites.Analytics
{
    internal class URLAnalyticsModel
    {
        public int ID { get; set; }

        public required string MostUsedLocationsJSON { get; set; }
        public required string MostUsedDevicesJSON { get; set; }

        public int ClickCount { get; set; } = 0;
        public DateTime LastTimeCalculated { get; set; }

        public int URLID { get; set; }
        public required URLModel URL { get; set; }
    }
    public class URLAnalyticsDTO
    {
        public int ID { get; set; }

        public required string MostUsedLocationsJSON { get; set; }
        public required string MostUsedDevicesJSON { get; set; }

        public int ClickCount { get; set; } = 0;
        public DateTime LastTimeCalculated { get; set; }

        public int URLID { get; set; }
    }
}
