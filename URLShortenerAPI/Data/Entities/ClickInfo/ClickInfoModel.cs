using URLShortenerAPI.Data.Entities.URL;

namespace URLShortenerAPI.Data.Entities.ClickInfo
{
    internal class ClickInfoModel
    {
        public int ID { get; set; }
        public DateTime ClickedAt { get; set; }
        public required string IPAddress { get; set; }
        public required string UserAgent { get; set; }


        public int LocationID { get; set; }
        public LocationInfo? PossibleLocation { get; set; }
        public int DeviceInfoID { get; set; }
        public DeviceInfo? DeviceInfo { get; set; }
        public int URLID { get; set; }
        public required URLModel URL { get; set; }
    }

    internal class LocationInfo
    {
        public int ID { get; set; }
        public required string City { get; set; }
        public required string Region { get; set; }
        public required string Country { get; set; }
        public required string CountryCode { get; set; }
        public required string Continent { get; set; }
        public required string Latitude { get; set; }
        public required string Longitude { get; set; }

        public int ClickID { get; set; }
        public ClickInfoModel? ClickInfo { get; set; }
    }

    internal class DeviceInfo
    {
        public int ID { get; set; }
        public string? OS { set; get; }
        public string? ClientInfo { get; set; }
        public string? Brand { get; set; }
        public bool IsBot { get; set; }
        public string? BotInfo { get; set; }
        public string? Model { get; set; }

        public int ClickID { get; set; }
        public ClickInfoModel? ClickInfo { get; set; }
    }
}