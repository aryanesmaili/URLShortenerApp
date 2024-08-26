namespace URLShortenerAPI.Data.Entites
{
    internal class ClickInfoModel
    {
        public int ID { get; set; }
        public DateTime ClickedAt { get; set; }
        public required string IPAddress { get; set; }
        public required string UserAgent { get; set; }
        public required string PossibleLocation { get; set; } // TODO: Detect this based on IP address
        public required DeviceOS DeviceOS { get; set; } // TODO: Detect this based on Either route or User agent

        public int URLID { get; set; }
        public required URLModel URL { get; set; }
    }
    public enum DeviceOS
    {
        Android,
        IOS,
        Windows,
        Linux,
        MacOS,
        Other
    }
}
