using URLShortener.Application.Features.Users.DTOs;

namespace URLShortener.Application.Features.Users.Interfaces.Services;

public interface IUserStatsService
{
    Task<UserDashboardDTO> GetDashboardByIDAsync(long userID);
    Task<UserStats> GetUserStats(long userID);
}
