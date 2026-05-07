using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using URLShortener.Application.Common.Interfaces.Repositories;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories;

public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T> where T : class
{
    private readonly AppDbContext _context = context;
    private readonly DbSet<T> _dbSet = context.Set<T>();
    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public void AddRange(IEnumerable<T> entities)
    {
        _dbSet.AddRange(entities);
    }

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.AnyAsync(predicate);
    }

    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate == null ? _dbSet.CountAsync() : _dbSet.CountAsync(predicate);
    }

    public async Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = false)
    {
        return asNoTracking
            ? await _dbSet.AsNoTracking().ToListAsync()
            : await _dbSet.ToListAsync();
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null, Func<IQueryable<T>, IQueryable<T>>? orderBy = null, bool asNoTracking = false)
    {
        IQueryable<T> query = _dbSet;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (include != null)
            query = include(query);

        if (orderBy != null)
            query = orderBy(query);

        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IQueryable<T>>? include = null, Func<IQueryable<T>, IQueryable<T>>? orderBy = null, bool asNoTracking = false)
    {
        IQueryable<T> query = _dbSet;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (include != null)
            query = include(query);

        if (predicate != null)
            query = query.Where(predicate);

        if (orderBy != null)
            query = orderBy(query);

        return await query.ToListAsync();
    }

    public IQueryable<TResult> Select<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> selector)
    {
        return _context.Set<T>()
            .Where(filter)
            .Select(selector);
    }

    public IQueryable<T> Query(bool asNoTracking = false)
    {
        return asNoTracking ? _dbSet.AsQueryable().AsNoTracking() : _dbSet.AsQueryable();
    }

    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

}
