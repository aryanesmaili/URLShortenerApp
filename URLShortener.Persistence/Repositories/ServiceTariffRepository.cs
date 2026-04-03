using URLShortener.Application.Repositories;
using URLShortener.Domain.Entities.ServiceTariffs;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories
{
    public sealed class ServiceTariffRepository : GenericRepository<ServiceTariffModel>, IServiceTariffRepository
    {
        private readonly AppDbContext _context;

        public ServiceTariffRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
