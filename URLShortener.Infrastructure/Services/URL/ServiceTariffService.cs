using URLShortener.Application.Interfaces.Infrastructure.External;
using URLShortener.Application.Interfaces.Services.URL;
using URLShortener.Application.Repositories;
using URLShortener.Application.Utility.Exceptions;
using URLShortener.Domain.Entities.ServiceTariffs;
using URLShortener.Domain.Enums;

namespace URLShortener.Infrastructure.Services.URL
{
    public sealed class ServiceTariffService : IServiceTariffService
    {
        private readonly ICacheService _cacheService;
        private readonly IServiceTariffRepository _serviceTariffRepository;
        private readonly IUnitOfWork _uow;

        public ServiceTariffService(ICacheService cacheService, IServiceTariffRepository serviceTariffRepository, IUnitOfWork uow)
        {
            _cacheService = cacheService;
            _serviceTariffRepository = serviceTariffRepository;
            _uow = uow;
        }

        /// <inheritdoc/>
        public async Task<decimal> GetServicePrice(ServiceType serviceType)
        {
            // first we try to read from cache.
            var priceTariff = await _cacheService.GetValueAsync<ServiceTariffModel>(((int)serviceType).ToString());
            if (priceTariff != null)
                return priceTariff.Price;

            // if it doesn't exist in Cache, we get it in a query from database and store it in cache.
            priceTariff = await _serviceTariffRepository.GetAsync(x => x.ID == (int)serviceType)
                ?? throw new NotFoundException(nameof(ServiceTariffModel), nameof(ServiceTariffModel.ID), (int)serviceType);

            await _cacheService.SetAsync(((int)serviceType).ToString(), priceTariff);

            return priceTariff.Price;
        }

        /// <inheritdoc/>
        public async Task UpdateServicePrice(ServiceType serviceType, decimal newPrice)
        {
            // we start updating from first updating the DB.
            var priceTariff = await _serviceTariffRepository.GetAsync(x => x.ID == (int)serviceType)
                ?? throw new NotFoundException(nameof(ServiceTariffModel), nameof(ServiceTariffModel.ID), (int)serviceType);

            priceTariff.Price = newPrice;
            await _uow.SaveChangesAsync();

            // now we update the cache to store the new price
            await _cacheService.SetAsync(((int)serviceType).ToString(), priceTariff, strategy: CacheStrategy.Always);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ServiceTariffModel>> GetServiceTariffs()
        {
            // first we lookup cache to see if we have the collection cached.
            var priceTariffs = await _cacheService.GetCollectionAsync<ServiceTariffModel>();

            if (priceTariffs != null) // if we had the collection in cache
                return [.. priceTariffs]; // we return it

            // we don't have it in cache, so we query DB and set it in cache for future use.
            priceTariffs = (await _serviceTariffRepository.GetAllAsync()).ToList();

            if (priceTariffs.Count == 0) // if we don't have anything in DB, return empty list.
                return [];

            // set the collection in cache as key:[collectionOfItems]
            await _cacheService.SetCollectionAsync<ServiceTariffModel>(priceTariffs, span: TimeSpan.FromMinutes(30));

            // return DB Result
            return [.. priceTariffs];
        }
    }
}
