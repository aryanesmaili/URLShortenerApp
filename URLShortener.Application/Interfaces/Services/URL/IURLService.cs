using URLShortener.Application.DTOs.EntityDTOs.URL;
using URLShortener.Common.Responses;
using URLShortener.Domain.Entities.URL;

namespace URLShortener.Application.Interfaces.Services.URL;

public interface IURLService
{
    Task<URLDTO> GetURLAsync(long urlID);
    Task<PagedResult<URLDTO>> GetPagedURLsAsync(long userId, int pageNumber, int pageSize);
    Task<URLShortenResponse> AddURL(URLCreateDTO url, long authenticatedUserId);
    Task<IReadOnlyList<URLShortenResponse>> AddBatchURL(BatchURLCreateDTO batchURL, long authenticatedUserId);
    Task ToggleStateAsync(long urlId, Action<URLModel> toggleAction, long userId);
    Task DeleteURL(long urlId, long userId);
}