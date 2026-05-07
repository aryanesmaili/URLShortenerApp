using URLShortener.Domain.Entities.ServiceTariffs;
using URLShortener.Domain.Enums;

namespace URLShortener.Application.Features.URLs.Interfaces.Services;

public interface IServiceTariffService
{
    Task<long> GetServicePrice(ServiceType serviceType);

    Task<IReadOnlyList<ServiceTariffModel>> GetServiceTariffs();

    Task UpdateServicePrice(ServiceType serviceType, long newPrice);
}
