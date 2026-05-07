using System.Text.Json.Serialization;

namespace URLShortener.Application.Features.URLs.DTOs;

public sealed record URLCreateDTO : URLCreateDTOBase
{
    public bool IsActive { get; init; } = true;
    public bool IsMonetized { get; init; } = false;
    public string? Description { get; init; }
    private string? customShortCode;
    public string? CustomShortCode
    {
        get => customShortCode;
        init => customShortCode = value?.Trim();
    }

    private List<string> categories = [];
    public List<string> Categories
    {
        get => categories;
        init => categories = value?.Select(x => x.Trim()).ToList() ?? [];
    }

    [JsonIgnore]
    public bool IsCustom => !string.IsNullOrEmpty(CustomShortCode);
    [JsonIgnore]
    public bool HasCategories => Categories.Count > 0;
}
