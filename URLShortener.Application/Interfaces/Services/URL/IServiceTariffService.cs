using URLShortener.Domain.Entities.ServiceTariffs;
using URLShortener.Domain.Enums;

namespace URLShortener.Application.Interfaces.Services.URL
{
    public interface IServiceTariffService
    {
        Task<decimal> GetServicePrice(ServiceType serviceType);

        Task<IReadOnlyList<ServiceTariffModel>> GetServiceTariffs();

        Task UpdateServicePrice(ServiceType serviceType, decimal newPrice);
    }

}
