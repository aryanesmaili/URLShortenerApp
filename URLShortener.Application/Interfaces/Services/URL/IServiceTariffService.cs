using URLShortener.Domain.Entities.ServiceTariffs;
using URLShortener.Domain.Enums;

namespace URLShortener.Application.Interfaces.Services.URL;

public interface IServiceTariffService
{
    Task<long> GetServicePrice(ServiceType serviceType);

    Task<IReadOnlyList<ServiceTariffModel>> GetServiceTariffs();

    Task UpdateServicePrice(ServiceType serviceType, long newPrice);
}
