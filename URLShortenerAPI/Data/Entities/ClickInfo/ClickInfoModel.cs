using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
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
        public required LocationInfo PossibleLocation { get; set; }
        public int DeviceInfoID { get; set; }
        public required DeviceInfo DeviceInfo { get; set; }
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
        public required string BrowserFamily { get; set; }
        public required ClientInfo Client { get; set; }
        public required Device Device { get; set; }
        public required OSInfo OS { get; set; }
        public required string OSFamily { get; set; }
        public required string UserAgent { get; set; }

        public int ClickID { get; set; }
        public ClickInfoModel? ClickInfo { get; set; }
    }

    [Owned]
    internal class ClientInfo
    {
        public string? Engine { get; set; }
        public string? EngineVersion { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Version { get; set; }
    }

    [Owned]
    internal class Device
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Type { get; set; }
    }

    [Owned]
    internal class OSInfo
    {
        public string? Name { get; set; }
        public string? Platform { get; set; }
        public string? Version { get; set; }
    }
}