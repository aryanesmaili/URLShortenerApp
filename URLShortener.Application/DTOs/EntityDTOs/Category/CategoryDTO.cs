using URLShortener.Application.DTOs.EntityDTOs.URL;

namespace URLShortener.Application.DTOs.EntityDTOs.Category;

public sealed class CategoryDTO
{
    public int ID { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }

    public int UserID { get; set; }
    public List<URLSummaryDTO>? URLs { get; set; }
}
