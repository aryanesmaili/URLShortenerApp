using SharedDataModels.Responses;
using URLShortener.Application.DTOs;
using URLShortener.Common.CustomClasses;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IURLTableService
    {
        Task<APIResponse<PagedResult<URLDTO>>> GetProfileURLList(int userID, int pageNumber, int pageSize);
    }
}