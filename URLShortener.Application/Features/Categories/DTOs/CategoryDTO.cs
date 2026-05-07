using URLShortener.Application.Features.URLs.DTOs;

namespace URLShortener.Application.Features.Categories.DTOs;

public sealed record CategoryDTO
{
    public int ID { get; init; }

    private string _title = string.Empty;
    public required string Title
    {
        get => _title;
        init
        {
            _title = value.Trim();
        }
    }
    private string? description;
    public string? Description
    {
        get => description;
        init => description = value?.Trim();
    }
    public int UserID { get; init; }
    public List<URLSummaryDTO>? URLs { get; init; }
}
