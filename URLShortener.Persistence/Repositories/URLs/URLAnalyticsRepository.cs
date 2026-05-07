using URLShortener.Application.Features.URLs.Interfaces.Repositories;
using URLShortener.Domain.Entities.Analytics;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories.URLs;

public sealed class URLAnalyticsRepository(AppDbContext context) : GenericRepository<URLAnalyticsModel>(context), IURLAnalyticsRepository
{
}
