using URLShortener.Application.Features.Categories.DTOs;

namespace URLShortener.Application.Features.URLs.DTOs;

public sealed record URLDTO
{
    public int ID { get; init; }
    public string? Description { get; init; }
    public string ShortCode { get; init; } = string.Empty;
    public string LongURL { get; init; } = string.Empty;
    public int ClickCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsActive { get; init; }
    public bool IsMonetized { get; init; }

    public int UserID { get; init; }
    public List<CategorySummaryDTO>? Categories { get; init; }
    public int? URLAnalyticsID { get; init; }
}
