using SharedDataModels.Responses;

namespace URLShortener.Common.Responses;

/// <summary>
/// A standardized response model for paginated API endpoints, extending the base APIResponse to include pagination metadata such as total count, page number, page size, and total pages. This structure allows for consistent handling of paginated responses across the application, making it easier to manage success and failure cases in a unified way while providing necessary information for client-side pagination controls.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PagedResult<T> : APIResponse<List<T>>
{
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
