using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.EntityDTOs.URL;

public sealed class URLCreateDTO
{
    [Required]
    public required string LongURL { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsMonetized { get; set; } = true;
    public string CustomShortCode { get; set; } = string.Empty;
    public string? Categories { get; set; }

    public bool IsCustom => !string.IsNullOrEmpty(CustomShortCode);
    public bool HasCategories => !string.IsNullOrEmpty(Categories);
}
