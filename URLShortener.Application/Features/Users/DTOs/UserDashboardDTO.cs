using URLShortener.Application.Features.URLs.DTOs;

namespace URLShortener.Application.Features.Users.DTOs;

public sealed record UserDashboardDTO
{
    public IReadOnlyDictionary<string, int> MonthlyChartData { get; init; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> HourlyChartData { get; init; } = new Dictionary<string, int>();
    public IReadOnlyList<string> TopCountries { get; init; } = [];
    public IReadOnlyList<string> TopOSs { get; init; } = [];
    public IReadOnlyList<URLDTO> TopClickedURLs { get; init; } = [];
    public IReadOnlyList<URLDTO> MostRecentURLs { get; init; } = [];
}

