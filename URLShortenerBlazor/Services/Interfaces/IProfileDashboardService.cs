using SharedDataModels.DTOs;
using SharedDataModels.Responses;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IProfileDashboardService
    {
        Task<APIResponse<UserDashboardDTO>> GetDashboardInfo(int userID);
    }
}
