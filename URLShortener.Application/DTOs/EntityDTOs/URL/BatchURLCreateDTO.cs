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

    public List<string> Categories { get; init; } = [];
    public string? Description { get; init; }
}

