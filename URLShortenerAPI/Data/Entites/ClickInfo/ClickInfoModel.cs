﻿using URLShortenerAPI.Data.Entites.URL;

namespace URLShortenerAPI.Data.Entites.ClickInfo
{
    internal class ClickInfoModel
    {
        public int ID { get; set; }
        public DateTime ClickedAt { get; set; }
        public required string IPAddress { get; set; }
        public required string UserAgent { get; set; }


        public int LocationID { get; set; }
        public required LocationInfo PossibleLocation { get; set; } // TODO: Detect this based on IP address
        public int DeviceInfoID { get; set; }
        public required DeviceInfo DeviceInfo { get; set; } // TODO: Detect this based on User agent
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

        public required int ClickID { get; set; }
        public required ClickInfoModel ClickInfo { get; set; }
    }

    internal class DeviceInfo
    {
        public int ID { get; set; }
        public required string OS { set; get; }
        public required string Browser { get; set; }
        public required string Brand { get; set; }

        public required int ClickID { get; set; }
        public required ClickInfoModel ClickInfo {  get; set; }
    }
}