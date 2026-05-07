using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using URLShortener.Application.Common.Interfaces.Infrastructure;
using URLShortener.Application.Features.URLs.DTOs;
using URLShortener.Application.Features.URLs.Interfaces.Repositories;
using URLShortener.Application.Features.Users.DTOs;
using URLShortener.Application.Features.Users.Interfaces.Services;
using URLShortenerAPI.Data;

namespace URLShortener.Infrastructure.Services.User;

/// <summary>
/// Service for retrieving and calculating user statistics including URL counts, click analytics, and dashboard data.
/// Implements caching strategy to optimize database queries and improve performance.
/// </summary>
public sealed class UserStatsService : IUserStatsService
{
    private readonly IURLRepository _urlRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    /// <summary>
    /// Static timezone reference for Iran Standard Time used throughout statistics calculations.
    /// </summary>
    private static readonly TimeZoneInfo _appTimeZone =
    TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");

    /// <summary>
    /// Initializes a new instance of the <see cref="UserStatsService"/> class.
    /// </summary>
    /// <param name="mapper">AutoMapper instance for DTO projections.</param>
    /// <param name="dbContextFactory">Factory for creating database context instances.</param>
    /// <param name="urlRepository">Repository for URL data access.</param>
    /// <param name="cacheService">Service for caching statistics and dashboard data.</param>
    public UserStatsService(
        IMapper mapper,
        IDbContextFactory<AppDbContext> dbContextFactory,
        IURLRepository urlRepository,
        ICacheService cacheService)
    {
        _mapper = mapper;
        _dbContextFactory = dbContextFactory;
        _urlRepository = urlRepository;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Retrieves aggregate statistics for a specific user including total URLs, average clicks, and growth metrics.
    /// </summary>
    /// <param name="userID">The unique identifier of the user.</param>
    /// <returns>A <see cref="UserStats"/> object containing aggregated user statistics.</returns>
    /// <remarks>
    /// Results are cached for 30 minutes to reduce database load.
    /// Statistics include: total URLs count, average clicks per URL, yesterday's clicks, and weekly growth percentage.
    /// </remarks>
    public async Task<UserStats> GetUserStats(long userID)
    {
        UserStats? stats = await _cacheService.GetValueAsync<UserStats>(userID.ToString());
        if (stats != null)
            return stats;

        stats = await FetchStatsFromDb(userID);

        await _cacheService.SetAsync(userID.ToString(), stats, TimeSpan.FromMinutes(30));
        return stats;
    }

    /// <summary>
    /// Fetches user statistics directly from the database without cache.
    /// </summary>
    /// <param name="userID">The unique identifier of the user.</param>
    /// <returns>A <see cref="UserStats"/> object with calculated metrics.</returns>
    /// <remarks>
    /// Calculates week boundaries and yesterday's UTC range to aggregate clicks data.
    /// Returns empty UserStats if no data exists for the user.
    /// </remarks>
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

    /// <summary>
    /// Calculates the start dates for the current week and previous week.
    /// Week starts on Saturday and ends on Friday per Iranian calendar convention.
    /// </summary>
    /// <returns>A tuple containing the start of this week and start of last week in UTC.</returns>
    private static (DateTime startOfThisWeek, DateTime startOfLastWeek) GetWeekBoundaries()
    {
        DateTime now = DateTime.UtcNow;
        // Calculate days since Saturday (Saturday = 0, Sunday = 1, ..., Friday = 6)
        int daysSinceSaturday = ((int)now.DayOfWeek + 1) % 7;
        DateTime startOfThisWeek = now.AddDays(-daysSinceSaturday).Date;
        return (startOfThisWeek, startOfThisWeek.AddDays(-7));
    }

    /// <summary>
    /// Calculates the UTC time range for yesterday based on Iran Standard Time.
    /// </summary>
    /// <returns>A tuple containing the start and end UTC times of yesterday in Iran timezone.</returns>
    /// <remarks>
    /// Converts current UTC time to Iran timezone, calculates yesterday's boundaries,
    /// then converts back to UTC for database queries.
    /// </remarks>
    private static (DateTime yesterdayStart, DateTime yesterdayEnd) GetYesterdayUtcRange()
    {
        TimeZoneInfo tehranTz = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
        DateTime todayTehran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tehranTz).Date;
        DateTime yesterdayStart = TimeZoneInfo.ConvertTimeToUtc(todayTehran.AddDays(-1), tehranTz);
        DateTime yesterdayEnd = TimeZoneInfo.ConvertTimeToUtc(todayTehran, tehranTz);
        return (yesterdayStart, yesterdayEnd);
    }

    /// <summary>
    /// Calculates the percentage growth of clicks between two weeks.
    /// </summary>
    /// <param name="thisWeek">The number of clicks in the current week.</param>
    /// <param name="lastWeek">The number of clicks in the previous week.</param>
    /// <returns>The percentage growth rounded to 2 decimal places. Returns 0 if both weeks have no clicks, 100 if last week had no clicks.</returns>
    private static double CalculateWeeklyGrowth(int thisWeek, int lastWeek)
    {
        if (thisWeek == 0 && lastWeek == 0) return 0;
        if (lastWeek == 0) return 100;
        return Math.Round((double)(thisWeek - lastWeek) / lastWeek * 100, 2);
    }

    /// <summary>
    /// Retrieves comprehensive dashboard data for a user including recent URLs, click charts, and top statistics.
    /// </summary>
    /// <param name="userID">The unique identifier of the user.</param>
    /// <returns>A <see cref="UserDashboardDTO"/> containing all dashboard components.</returns>
    /// <remarks>
    /// Executes all data fetch operations concurrently for better performance.
    /// Results are cached for 1 minute to ensure relatively fresh data while reducing database hits.
    /// Dashboard includes hourly and monthly click charts, recent URLs, top countries, operating systems, and clicked URLs.
    /// </remarks>
    public async Task<UserDashboardDTO> GetDashboardByIDAsync(long userID)
    {
        var cached = await _cacheService.GetValueAsync<UserDashboardDTO>(userID.ToString());
        if (cached != null)
            return cached;

        var recentURLsTask = GetRecentURLs(userID);
        var clicksThisMonthTask = GetClicksInMonth(userID);
        var clicksThisDayTask = GetClicksInDay(userID);
        var topCountriesTask = GetTopCountries(userID);
        var topDevicesTask = GetTopDeviceOS(userID);
        var topClickedURLsTask = GetTopClickedURLs(userID);

        // Execute all queries concurrently
        await Task.WhenAll(recentURLsTask, clicksThisMonthTask, clicksThisDayTask,
                           topCountriesTask, topDevicesTask, topClickedURLsTask);

        var result = new UserDashboardDTO
        {
            HourlyChartData = await clicksThisDayTask,
            MonthlyChartData = await clicksThisMonthTask,
            MostRecentURLs = await recentURLsTask,
            TopCountries = await topCountriesTask,
            TopOSs = await topDevicesTask,
            TopClickedURLs = await topClickedURLsTask,
        };

        await _cacheService.SetAsync(userID.ToString(), result, TimeSpan.FromMinutes(1));
        return result;
    }

    /// <summary>
    /// Retrieves the top clicked URLs for a user sorted by click count.
    /// </summary>
    /// <param name="userID">The unique identifier of the user.</param>
    /// <param name="count">The maximum number of URLs to return. Default is 5.</param>
    /// <returns>A read-only list of <see cref="URLDTO"/> objects sorted by click count in descending order.</returns>
    /// <remarks>
    /// Results are cached for 5 minutes using a composite key including user ID and count parameter.
    /// </remarks>
    private Task<IReadOnlyList<URLDTO>> GetTopClickedURLs(long userID, int count = 5)
    {
        string key = $"TopClicked_{userID}_{count}";

        return GetCachedAsync<IReadOnlyList<URLDTO>>(key, async () =>
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var list = await db.URLs
                .AsNoTracking()
                .Where(x => x.UserID == userID)
                .OrderByDescending(x => x.ClickCount)
                .Take(count)
                .ProjectTo<URLDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return list.AsReadOnly();
        });
    }

    /// <summary>
    /// Retrieves the most recently created URLs for a user.
    /// </summary>
    /// <param name="userID">The unique identifier of the user.</param>
    /// <param name="count">The maximum number of URLs to return. Default is 5.</param>
    /// <returns>A read-only list of <see cref="URLDTO"/> objects sorted by creation date in descending order.</returns>
    /// <remarks>
    /// Results are cached for 5 minutes using a composite key including user ID and count parameter.
    /// </remarks>
    private Task<IReadOnlyList<URLDTO>> GetRecentURLs(long userID, int count = 5)
    {
        string key = $"RecentURLs_{userID}_{count}";

        return GetCachedAsync<IReadOnlyList<URLDTO>>(key, async () =>
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var list = await db.URLs
                .AsNoTracking()
                .Where(x => x.UserID == userID)
                .OrderByDescending(x => x.CreatedAt)
                .Take(count)
                .ProjectTo<URLDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return list.AsReadOnly();
        });
    }

    /// <summary>
    /// Retrieves the top countries where a user's URLs were clicked.
    /// </summary>
    /// <param name="userID">The unique identifier of the user.</param>
    /// <param name="count">The maximum number of countries to return. Default is 5.</param>
    /// <returns>A read-only list of country names sorted by click count in descending order.</returns>
    /// <remarks>
    /// Filters out clicks without location information.
    /// Results are cached for 5 minutes using a composite key including user ID and count parameter.
    /// </remarks>
    private Task<IReadOnlyList<string>> GetTopCountries(long userID, int count = 5)
    {
        string key = $"TopCountry_{userID}_{count}";

        return GetCachedAsync<IReadOnlyList<string>>(key, async () =>
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var list = await db.Clicks
                .AsNoTracking()
                .Where(x => x.URL.UserID == userID &&
                            x.PossibleLocation != null &&
                            x.PossibleLocation.Country != null)
                .GroupBy(x => x.PossibleLocation!.Country)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            return list.AsReadOnly();
        });
    }

    /// <summary>
    /// Retrieves the top operating systems used to click on a user's URLs.
    /// </summary>
    /// <param name="userID">The unique identifier of the user.</param>
    /// <param name="count">The maximum number of operating systems to return. Default is 5.</param>
    /// <returns>A read-only list of operating system names sorted by click count in descending order.</returns>
    /// <remarks>
    /// Filters out clicks without device information.
    /// Results are cached for 5 minutes using a composite key including user ID and count parameter.
    /// </remarks>
    private Task<IReadOnlyList<string>> GetTopDeviceOS(long userID, int count = 5)
    {
        string key = $"TopOS_{userID}_{count}";

        return GetCachedAsync<IReadOnlyList<string>>(key, async () =>
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var list = await db.Clicks
                .AsNoTracking()
                .Where(x => x.URL.UserID == userID &&
                            x.DeviceInfo != null &&
                            x.DeviceInfo.OS != null &&
                            x.DeviceInfo.OS.Name != null)
                .GroupBy(x => x.DeviceInfo!.OS!.Name!)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            return list.AsReadOnly();
        });
    }

    /// <summary>
    /// Retrieves hourly click distribution for today based on Iran Standard Time.
    /// </summary>
    /// <param name="userID">The unique identifier of the user.</param>
    /// <returns>A read-only dictionary with hour (0-23) as key and click count as value.</returns>
    /// <remarks>
    /// Converts UTC click times to Iran timezone before grouping by hour.
    /// Hours with no clicks are included with a count of 0.
    /// Results are cached for 5 minutes.
    /// </remarks>
    private Task<IReadOnlyDictionary<string, int>> GetClicksInDay(long userID)
    {
        string key = $"D_Clicks_{userID}";

        return GetCachedAsync<IReadOnlyDictionary<string, int>>(key, async () =>
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            // Convert current UTC time to Iran timezone to get today's date
            var nowIran = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _appTimeZone);
            var startOfDayIran = nowIran.Date;
            var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayIran, _appTimeZone);
            var endOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayIran.AddDays(1), _appTimeZone);

            var clicks = await db.Clicks
                .AsNoTracking()
                .Where(x => x.URL.UserID == userID &&
                            x.ClickedAt >= startOfDayUtc &&
                            x.ClickedAt < endOfDayUtc)
                .Select(x => x.ClickedAt)
                .ToListAsync();

            // Initialize dictionary with all 24 hours
            var dict = Enumerable.Range(0, 24).ToDictionary(h => h.ToString(), _ => 0);

            // Aggregate clicks by hour in Iran timezone
            foreach (var clickedAt in clicks)
            {
                var localHour = TimeZoneInfo.ConvertTimeFromUtc(clickedAt, _appTimeZone).Hour;
                dict[localHour.ToString()]++;
            }

            return dict;
        });
    }

    /// <summary>
    /// Retrieves daily click distribution for the current Persian calendar month.
    /// </summary>
    /// <param name="userID">The unique identifier of the user.</param>
    /// <returns>A read-only dictionary with day of month (1-31) as key and click count as value.</returns>
    /// <remarks>
    /// Uses Persian calendar for month calculations. Days without clicks are included with a count of 0.
    /// Cache TTL is dynamically calculated as the time remaining until the end of the current month.
    /// </remarks>
    private Task<IReadOnlyDictionary<string, int>> GetClicksInMonth(long userID)
    {
        string key = $"M_Clicks_{userID}";

        var now = DateTime.UtcNow;
        var calendar = new PersianCalendar();
        int py = calendar.GetYear(now);
        int pm = calendar.GetMonth(now);

        // Get number of days in current Persian month
        int daysInMonth = calendar.GetDaysInMonth(py, pm);
        var startOfMonth = calendar.ToDateTime(py, pm, 1, 0, 0, 0, 0);
        var endOfMonth = pm == 12
            ? calendar.ToDateTime(py + 1, 1, 1, 0, 0, 0, 0)
            : calendar.ToDateTime(py, pm + 1, 1, 0, 0, 0, 0);

        // Calculate time-to-live as remaining time until end of month
        var ttl = endOfMonth - now;

        return GetCachedAsync<IReadOnlyDictionary<string, int>>(key, async () =>
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var clickDates = await db.Clicks
                .AsNoTracking()
                .Where(x => x.URL.UserID == userID &&
                            x.ClickedAt >= startOfMonth &&
                            x.ClickedAt < endOfMonth)
                .Select(x => x.ClickedAt)
                .ToListAsync();

            // Initialize dictionary with all days in the Persian month
            var dict = Enumerable.Range(1, daysInMonth)
                .ToDictionary(d => d.ToString(), _ => 0);

            // Aggregate clicks by day of month using Persian calendar
            foreach (var date in clickDates)
            {
                var day = calendar.GetDayOfMonth(date).ToString();
                dict[day]++;
            }

            return dict;
        }, ttl);
    }

    /// <summary>
    /// Generic method for retrieving cached data or fetching from database if cache miss.
    /// </summary>
    /// <typeparam name="TResult">The type of data to cache and retrieve.</typeparam>
    /// <param name="cacheKey">The cache key for storing/retrieving the data.</param>
    /// <param name="queryFunc">Async function that fetches data from database if cache miss occurs.</param>
    /// <param name="ttl">Optional time-to-live for cache entry. Defaults to 5 minutes if not specified.</param>
    /// <returns>The cached data if available, otherwise the result of executing queryFunc.</returns>
    /// <remarks>
    /// Implements cache-aside pattern. If cache miss occurs, executes queryFunc and caches the result
    /// before returning. TResult must be a reference type (class).
    /// </remarks>
    private async Task<TResult> GetCachedAsync<TResult>(string cacheKey, Func<Task<TResult>> queryFunc, TimeSpan? ttl = null)
        where TResult : class
    {
        var cached = await _cacheService.GetValueAsync<TResult>(cacheKey);
        if (cached != null)
            return cached;

        var result = await queryFunc();

        await _cacheService.SetAsync(cacheKey, result, ttl ?? TimeSpan.FromMinutes(5));
        return result;
    }
}
