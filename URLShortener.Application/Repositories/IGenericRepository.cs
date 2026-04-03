using System.Linq.Expressions;

namespace URLShortener.Application.Repositories;

public interface IGenericRepository<T> where T : class
{
    // Query
    IQueryable<T> Query(bool asNoTracking = false);
    Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = false);

    Task<T?> GetAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        bool asNoTracking = false);

    Task<IEnumerable<T>> GetListAsync(
    Expression<Func<T, bool>>? predicate = null,
    Func<IQueryable<T>, IQueryable<T>>? include = null,
    Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
    bool asNoTracking = false);

    IQueryable<TResult> Select<TResult>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TResult>> selector);

    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    // Command
    void Add(T entity);
    void AddRange(IEnumerable<T> entities);

    void Update(T entity);

    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
