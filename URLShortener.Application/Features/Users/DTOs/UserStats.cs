namespace URLShortener.Application.Features.Users.DTOs;

public sealed class UserStats
{
    public int TotalURLsCount { get; set; }
    public int ClicksYesterdayCount { get; set; }
    public double WeeklyGrowth { get; set; }
    public double AverageClicksPerURL { get; set; }
}

