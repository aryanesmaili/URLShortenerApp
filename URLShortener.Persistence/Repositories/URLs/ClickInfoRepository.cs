using URLShortener.Application.Features.URLs.Interfaces.Repositories;
using URLShortener.Domain.Entities.ClickInfo;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories.URLs;

public sealed class ClickInfoRepository(AppDbContext context) : GenericRepository<ClickInfoModel>(context), IClickInfoRepository
{
}
