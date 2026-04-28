namespace URLShortener.Application.DTOs.EntityDTOs;

public sealed record GetPagedItemsRequest
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
