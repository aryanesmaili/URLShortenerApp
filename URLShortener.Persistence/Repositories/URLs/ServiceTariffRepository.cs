using URLShortener.Application.Features.URLs.Interfaces.Repositories;
using URLShortener.Domain.Entities.ServiceTariffs;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories.URLs;

public sealed class ServiceTariffRepository(AppDbContext context) : GenericRepository<ServiceTariffModel>(context), IServiceTariffRepository
{
}
