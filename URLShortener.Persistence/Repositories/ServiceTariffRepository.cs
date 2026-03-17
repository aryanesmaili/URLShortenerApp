using System.Linq.Expressions;
using URLShortener.Application.Repositories;
using URLShortener.Domain.Entities.ServiceTariffs;

namespace URLShortener.Persistence.Repositories
{
    public sealed class ServiceTariffRepository : IServiceTariffRepository
    {
        public void Add(ServiceTariffModel entity)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(ServiceTariffModel entity)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<ServiceTariffModel> entities)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<ServiceTariffModel> entities)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync(Expression<Func<ServiceTariffModel, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(Expression<Func<ServiceTariffModel, bool>>? predicate = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ServiceTariffModel>> FindAsync(Expression<Func<ServiceTariffModel, bool>> predicate, Func<IQueryable<ServiceTariffModel>, IQueryable<ServiceTariffModel>>? include = null, Func<IQueryable<ServiceTariffModel>, IQueryable<ServiceTariffModel>>? orderBy = null, bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ServiceTariffModel>> GetAllAsync(bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceTariffModel?> GetAsync(Expression<Func<ServiceTariffModel, bool>> predicate, Func<IQueryable<ServiceTariffModel>, IQueryable<ServiceTariffModel>>? include = null, Func<IQueryable<ServiceTariffModel>, IQueryable<ServiceTariffModel>>? orderBy = null, bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceTariffModel?> GetByIdAsync(object id, bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ServiceTariffModel> Query(bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public void Remove(ServiceTariffModel entity)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<ServiceTariffModel> entities)
        {
            throw new NotImplementedException();
        }

        public void Update(ServiceTariffModel entity)
        {
            throw new NotImplementedException();
        }
    }
}
