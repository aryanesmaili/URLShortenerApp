namespace URLShortener.Application.Features.Paged.DTOs;

public sealed record GetPagedItemsRequest
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
