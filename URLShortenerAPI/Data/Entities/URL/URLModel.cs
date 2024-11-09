using URLShortenerAPI.Data.Entities.Analytics;
using URLShortenerAPI.Data.Entities.ClickInfo;
using URLShortenerAPI.Data.Entities.Finance;
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
        public int ClickCount { get; set; }
        public bool IsMonetized { get; set; }

        public int UserID { get; set; }
        public required UserModel User { get; set; }

        public ICollection<ClickInfoModel>? Clicks { get; set; }

        public int? CategoryID { get; set; }
        public ICollection<URLCategoryModel>? Categories { get; set; }

        public int URLAnalyticsID { get; set; }
        public URLAnalyticsModel? URLAnalytics { get; set; }

        public int? PurchaseID { get; set; }
        public PurchaseModel? Purchase { get; set; }
    }
}