using System.Linq.Expressions;
using URLShortener.Common.Responses;

namespace URLShortener.Application.Common.Interfaces.Repositories;

public interface IGetPaged<T> where T : class
{
    Task<PagedResult<T>> GetPagedAsync<TKey>(int pageNumber, int pageSize, Expression<Func<T, bool>>? ownershipFilter = null, Expression<Func<T, TKey>>? orderBy = null, bool descending = false);
}
