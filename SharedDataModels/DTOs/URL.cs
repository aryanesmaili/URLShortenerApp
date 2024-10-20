using System.ComponentModel.DataAnnotations;

namespace SharedDataModels.DTOs
{
    public class URLDTO
    {
        public int ID { get; set; }
        public required string Description { get; set; }
        public required string ShortCode { get; set; }
        public required string LongURL { get; set; }
        public required int ClickCount { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required bool IsActive { get; set; }

        public int UserID { get; set; }
        public int? CategoryID { get; set; }
        public int URLAnalyticsID { get; set; }
    }

    public class URLCreateDTO
    {
        [Required]
        public required string LongURL { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string? Category { get; set; }
        public required int UserID { get; set; }
    }

    public class BatchURLAdditionResponse
    {
        public List<URLDTO>? NewURLs { get; set; }
        public List<URLDTO>? ConflictedURLs { get; set; }
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
