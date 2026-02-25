using System.Linq.Expressions;

namespace URLShortener.Application.Repositories;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> Query(bool asNoTracking = false);

    // Read
    Task<T?> GetByIdAsync(object id, bool asNoTracking = false);
    Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = false);

    Task<T?> GetAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        bool asNoTracking = false);

    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        bool asNoTracking = false
    );

    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    // Write
    void Add(T entity);
    Task AddAsync(T entity);
    void AddRange(IEnumerable<T> entities);
    Task AddRangeAsync(IEnumerable<T> entities);

    void Update(T entity);

    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);

}
