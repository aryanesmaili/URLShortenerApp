using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using URLShortener.Application.Common.Interfaces.Repositories;
using URLShortener.Common.Responses;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories;

public sealed class GetPaged<T>(AppDbContext dbContext) : IGetPaged<T> where T : class
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<PagedResult<T>> GetPagedAsync<TKey>(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? ownershipFilter = null,
        Expression<Func<T, TKey>>? orderBy = null,
        bool descending = false)
    {
        var set = _dbContext.Set<T>().AsQueryable();

        if (ownershipFilter != null)
            set = set.Where(ownershipFilter);

        if (orderBy != null)
        {
            set = descending
                ? set.OrderByDescending(orderBy)
                : set.OrderBy(orderBy);
        }

        var totalCount = await set.CountAsync();

        var items = await set
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<T>
        {
            Items = items,
            TotalPages = totalPages,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

