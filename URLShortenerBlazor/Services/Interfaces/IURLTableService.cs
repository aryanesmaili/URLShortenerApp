using SharedDataModels.CustomClasses;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;

namespace URLShortenerBlazor.Services.Interfaces
{
    internal interface IURLTableService
    {
        Task<APIResponse<PagedResult<URLDTO>>> GetProfileURLList(int userID, int pageNumber, int pageSize);
    }
}