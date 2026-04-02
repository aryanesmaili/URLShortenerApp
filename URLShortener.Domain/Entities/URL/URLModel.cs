using URLShortener.Domain.Entities.Analytics;
using URLShortener.Domain.Entities.ClickInfo;
using URLShortener.Domain.Entities.Finance;
using URLShortener.Domain.Entities.URLCategory;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Domain.Entities.URL;

public class URLModel
{
    public long ID { get; set; }
    public string? Description { get; set; }
    public required string ShortCode { get; set; }
    public required string LongURL { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public int ClickCount { get; set; }
    public bool IsMonetized { get; set; }

    public long UserID { get; set; }
    public required UserModel User { get; set; }

    public ICollection<ClickInfoModel>? Clicks { get; set; } = [];

    public long? CategoryID { get; set; }
    public ICollection<URLCategoryModel>? Categories { get; set; }

    public long URLAnalyticsID { get; set; }
    public URLAnalyticsModel? URLAnalytics { get; set; }

    public long? PurchaseID { get; set; }
    public PurchaseModel? Purchase { get; set; }
}