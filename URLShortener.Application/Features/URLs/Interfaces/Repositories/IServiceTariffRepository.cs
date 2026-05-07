using URLShortener.Application.Common.Interfaces.Repositories;
using URLShortener.Domain.Entities.ServiceTariffs;

namespace URLShortener.Application.Features.URLs.Interfaces.Repositories;

public interface IServiceTariffRepository : IGenericRepository<ServiceTariffModel>
{ }
