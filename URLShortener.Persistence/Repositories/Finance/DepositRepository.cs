using URLShortener.Application.Common.Interfaces.Repositories;
using URLShortener.Application.Features.Finance.Interfaces.Repositories;
using URLShortener.Common.Responses;
using URLShortener.Domain.Entities.Finance;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories.Finance;

public sealed class DepositRepository(AppDbContext context, IGetPaged<DepositModel> pager) : GenericRepository<DepositModel>(context), IDepositRepository
{
    private readonly IGetPaged<DepositModel> _pager = pager;

    public Task<PagedResult<DepositModel>> GetPagedAsync<TKey>(int pageNumber, int pageSize, System.Linq.Expressions.Expression<Func<DepositModel, bool>>? ownershipFilter = null, System.Linq.Expressions.Expression<Func<DepositModel, TKey>>? orderBy = null, bool descending = false)
    {
        return _pager.GetPagedAsync(pageNumber, pageSize, ownershipFilter, orderBy, descending);
    }
}
