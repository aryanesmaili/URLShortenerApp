using URLShortener.Application.Common.Interfaces.Repositories;
using URLShortener.Domain.Entities.Finance;

namespace URLShortener.Application.Features.Finance.Interfaces.Repositories;

public interface IDepositRepository : IGenericRepository<DepositModel>, IGetPaged<DepositModel>
{
}
