using URLShortener.Application.DTOs.EntityDTOs.User;

namespace URLShortener.Application.Interfaces.Services.User;

public interface IUserStatsService
{
    Task<UserDashboardDTO> GetDashboardByIDAsync(long userID);
    public Task<UserStats> GetUserStats(long userID);
}
