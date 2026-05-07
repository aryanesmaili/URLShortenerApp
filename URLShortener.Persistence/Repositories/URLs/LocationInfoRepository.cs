using URLShortener.Application.Features.URLs.Interfaces.Repositories;
using URLShortener.Domain.Entities.ClickInfo;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories.URLs;

public sealed class LocationInfoRepository(AppDbContext context) : GenericRepository<LocationInfo>(context), ILocationInfoRepository
{
}
