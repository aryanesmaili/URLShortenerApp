using URLShortener.Application.DTOs.EntityDTOs.Category;

namespace URLShortener.Application.DTOs.EntityDTOs.URL;

public sealed class URLDTO
{
    public int ID { get; set; }
    public string? Description { get; set; }
    public string ShortCode { get; set; } = string.Empty;
    public string LongURL { get; set; } = string.Empty;
    public int ClickCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsMonetized { get; set; }

    public int UserID { get; set; }
    public List<CategorySummaryDTO>? Categories { get; set; }
    public int? URLAnalyticsID { get; set; }
}
