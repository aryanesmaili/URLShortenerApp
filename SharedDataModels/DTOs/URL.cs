using System.ComponentModel.DataAnnotations;

namespace SharedDataModels.DTOs
{
    public class URLDTO
    {
        public int ID { get; set; }
        public string? Description { get; set; }
        public string ShortCode { get; set; } = string.Empty;
        public string LongURL { get; set; } = string.Empty;
        public int ClickCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public int UserID { get; set; }
        public List<CategoryDTO>? Categories { get; set; }
        public int? URLAnalyticsID { get; set; }
    }

    public class URLCreateDTO
    {
        [Required]
        public required string LongURL { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsMonetized { get; set; } = true;
        public string CustomShortCode { get; set; } = string.Empty;
        public string? Categories { get; set; }
        public required int UserID { get; set; }
    }

    public class URLShortenResponse
    {
        public required URLDTO URL { get; set; }
        public bool IsNew { get; set; }
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
