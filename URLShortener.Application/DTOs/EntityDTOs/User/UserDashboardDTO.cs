using URLShortener.Application.DTOs.EntityDTOs.URL;

namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed class UserDashboardDTO
{
    public Dictionary<string, int>? MonthlyChartData { get; set; }
    public Dictionary<string, int>? HourlyChartData { get; set; }
    public List<string>? TopCountries { get; set; }
    public List<string>? TopOSs { get; set; }
    public List<URLDTO>? TopClickedURLs { get; set; }
    public List<URLDTO>? MostRecentURLs { get; set; }
}

