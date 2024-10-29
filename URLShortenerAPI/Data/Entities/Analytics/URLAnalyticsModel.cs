using System.Text.Json.Serialization;
using URLShortenerAPI.Data.Entities.URL;

namespace URLShortenerAPI.Data.Entities.Analytics
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

    public class IncomingRequestInfo
    {
        public required string IPAddress { get; set; }
        public required string UserAgent { get; set; }
        public required DateTime TimeClicked { get; set; }
        [JsonInclude]
        internal URLModel? URL { get; set; }
    }
}
