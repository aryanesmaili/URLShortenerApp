using System.Linq.Expressions;
using URLShortener.Application.Common.Interfaces.Repositories;
using URLShortener.Application.Features.URLs.Interfaces.Repositories;
using URLShortener.Common.Responses;
using URLShortener.Domain.Entities.URL;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories.URLs;

public sealed class URLRepository(AppDbContext context, IGetPaged<URLModel> pager) : GenericRepository<URLModel>(context), IURLRepository
{
    private readonly IGetPaged<URLModel> _pager = pager;

    public Task<PagedResult<URLModel>> GetPagedAsync<TKey>(int pageNumber, int pageSize, Expression<Func<URLModel, bool>>? ownershipFilter = null, Expression<Func<URLModel, TKey>>? orderBy = null, bool descending = false)
    {
        return _pager.GetPagedAsync(pageNumber, pageSize, ownershipFilter, orderBy, descending);
    }
}
