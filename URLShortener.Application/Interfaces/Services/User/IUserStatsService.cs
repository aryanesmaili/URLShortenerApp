using URLShortener.Application.DTOs.EntityDTOs.User;

namespace URLShortener.Application.Interfaces.Services.User;

public interface IUserStatsService
{
    public Task<UserStats> GetUserStats(long userID);
}
