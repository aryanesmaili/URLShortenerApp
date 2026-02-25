using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using URLShortener.Application.Repositories;
using URLShortenerAPI.Data;

namespace URLShortener.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public void Add(T entity)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<T> entities)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<T> entities)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null, Func<IQueryable<T>, IQueryable<T>>? orderBy = null, bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null, Func<IQueryable<T>, IQueryable<T>>? orderBy = null, bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetByIdAsync(object id, bool asNoTracking = false)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Query(bool asNoTracking = false)
        {
            return asNoTracking ? _dbSet.AsQueryable().AsNoTracking() : _dbSet.AsQueryable();
        }

        public void Remove(T entity)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            throw new NotImplementedException();
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
