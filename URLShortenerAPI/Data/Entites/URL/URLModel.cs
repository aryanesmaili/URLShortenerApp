using URLShortenerAPI.Data.Entites.Analytics;
using URLShortenerAPI.Data.Entites.ClickInfo;
using URLShortenerAPI.Data.Entites.URLCategory;
using URLShortenerAPI.Data.Entites.User;

namespace URLShortenerAPI.Data.Entites.URL
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
        public URLCategoryModel? Category { get; set; }

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
}