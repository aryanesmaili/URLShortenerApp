using URLShortener.Application.DTOs.EntityDTOs.URL;

namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed record UserDashboardDTO
{
    public IReadOnlyDictionary<string, int> MonthlyChartData { get; init; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> HourlyChartData { get; init; } = new Dictionary<string, int>();
    public IReadOnlyList<string> TopCountries { get; init; } = [];
    public IReadOnlyList<string> TopOSs { get; init; } = [];
    public IReadOnlyList<URLDTO> TopClickedURLs { get; init; } = [];
    public IReadOnlyList<URLDTO> MostRecentURLs { get; init; } = [];
}

