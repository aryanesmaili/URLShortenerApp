using Microsoft.EntityFrameworkCore;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.Interfaces.Infrastructure.External;
using URLShortener.Application.Interfaces.Services.User;
using URLShortener.Application.Repositories;
using URLShortenerAPI.Data;

namespace URLShortener.Infrastructure.Services.User;

public sealed class UserStatsService : IUserStatsService
{
    private readonly IURLRepository _urlRepository;
    private readonly ICacheService _cacheService;

    public UserStatsService(
        ICacheService cacheService,
        IURLRepository urlRepository)
    {
        _cacheService = cacheService;
        _urlRepository = urlRepository;
    }

    public async Task<UserStats> GetUserStats(long userID)
    {
        UserStats? stats = await _cacheService.GetValueAsync<UserStats>(userID.ToString());
        if (stats != null)
            return stats;

        stats = await FetchStatsFromDb(userID);

        await _cacheService.SetAsync(userID.ToString(), stats, TimeSpan.FromMinutes(30));
        return stats;
    }

    private async Task<UserStats> FetchStatsFromDb(long userID)
    {
        var (startOfThisWeek, startOfLastWeek) = GetWeekBoundaries();
        var (yesterdayStart, yesterdayEnd) = GetYesterdayUtcRange();

        var stats = await _urlRepository
            .Query()
            .Where(u => u.UserID == userID)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalURLs = g.Count(),
                TotalClicks = g.Sum(u => u.Clicks!.Count),
                YesterdayClicks = g.Sum(u => u.Clicks!.Count(c => c.ClickedAt >= yesterdayStart && c.ClickedAt < yesterdayEnd)),
                ThisWeekClicks = g.Sum(u => u.Clicks!.Count(c => c.ClickedAt >= startOfThisWeek)),
                LastWeekClicks = g.Sum(u => u.Clicks!.Count(c => c.ClickedAt >= startOfLastWeek && c.ClickedAt < startOfThisWeek)),
            })
            .FirstOrDefaultAsync();

        if (stats == null)
            return new UserStats();

        return new UserStats
        {
            TotalURLsCount = stats.TotalURLs,
            AverageClicksPerURL = stats.TotalURLs == 0 ? 0 : Math.Round((double)stats.TotalClicks / stats.TotalURLs, 2),
            ClicksYesterdayCount = stats.YesterdayClicks,
            WeeklyGrowth = CalculateWeeklyGrowth(stats.ThisWeekClicks, stats.LastWeekClicks)
        };
    }

    // DayOfWeek: Sun=0, Mon=1, ..., Sat=6
    // +1 mod 7 maps Saturday→0, Sunday→1, ..., Friday→6
    private static (DateTime startOfThisWeek, DateTime startOfLastWeek) GetWeekBoundaries()
    {
        DateTime now = DateTime.UtcNow;
        int daysSinceSaturday = ((int)now.DayOfWeek + 1) % 7;
        DateTime startOfThisWeek = now.AddDays(-daysSinceSaturday).Date;
        return (startOfThisWeek, startOfThisWeek.AddDays(-7));
    }

    private static (DateTime yesterdayStart, DateTime yesterdayEnd) GetYesterdayUtcRange()
    {
        TimeZoneInfo tehranTz = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
        DateTime todayTehran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tehranTz).Date;
        DateTime yesterdayStart = TimeZoneInfo.ConvertTimeToUtc(todayTehran.AddDays(-1), tehranTz);
        DateTime yesterdayEnd = TimeZoneInfo.ConvertTimeToUtc(todayTehran, tehranTz);
        return (yesterdayStart, yesterdayEnd);
    }

    private static double CalculateWeeklyGrowth(int thisWeek, int lastWeek)
    {
        if (thisWeek == 0 && lastWeek == 0) return 0;
        if (lastWeek == 0) return 100;
        return Math.Round((double)(thisWeek - lastWeek) / lastWeek * 100, 2);
    }
}
