using URLShortenerAPI.Data.Entities.Analytics;
using URLShortenerAPI.Data.Entities.ClickInfo;
using URLShortenerAPI.Data.Entities.URLCategory;
using URLShortenerAPI.Data.Entities.User;

namespace URLShortenerAPI.Data.Entities.URL
{
    internal class URLModel
    {
        public int ID { get; set; }
        public string? Description { get; set; }
        public required string ShortCode { get; set; }
        public required string LongURL { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }


        public int UserID { get; set; }
        public required UserModel User { get; set; }

        public ICollection<ClickInfoModel>? Clicks { get; set; }

        public int? CategoryID { get; set; }
        public ICollection<URLCategoryModel>? Categories { get; set; }

        public int URLAnalyticsID { get; set; }
        public URLAnalyticsModel? URLAnalytics { get; set; }
    }

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
        public required string LongURL { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string? Category { get; set; }
        public required int UserID { get; set; }
    }
}