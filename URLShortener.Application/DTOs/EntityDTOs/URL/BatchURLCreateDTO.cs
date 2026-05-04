namespace URLShortener.Application.DTOs.EntityDTOs.URL;

public sealed record BatchURLCreateDTO
{
    public BatchURLCreateDTO()
    {

    }
    public BatchURLCreateDTO(BatchURLCreateDTO original, IReadOnlyCollection<URLCreateDTOBase> newItems)
    {
        URLs = [.. newItems];
        IsActive = original.IsActive;
        IsMonetized = original.IsMonetized;
        Categories = original.Categories;
        Description = original.Description;
    }
    public List<URLCreateDTOBase> URLs { get; init; } = [];

    public bool IsActive { get; init; } = true;
    public bool IsMonetized { get; init; } = true;

    private List<string> categories = [];
    public List<string> Categories
    {
        get => categories;
        init => categories = value?.Select(x => x.Trim()).ToList() ?? [];
    }

    private string? description;
    public string? Description
    {
        get => description;
        init => description = value;
    }
}

