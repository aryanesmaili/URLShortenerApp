namespace URLShortener.Application.Features.Categories.DTOs;

public sealed record CategorySummaryDTO
{
    public int ID { get; set; }
    private string title = string.Empty;
    public required string Title
    {
        get => title;
        set => title = value.Trim();
    }
}