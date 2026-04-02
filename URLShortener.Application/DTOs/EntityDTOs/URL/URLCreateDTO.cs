namespace URLShortener.Application.DTOs.EntityDTOs.URL;

public sealed record URLCreateDTO : URLCreateDTOBase
{
    public bool IsActive { get; init; } = true;
    public bool IsMonetized { get; init; } = true;
    public string? Description { get; init; }
    public string? CustomShortCode { get; init; }
    public List<string> Categories { get; init; } = [];

    public bool IsCustom => !string.IsNullOrEmpty(CustomShortCode);
    public bool HasCategories => Categories.Count > 0;
}
