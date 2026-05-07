using URLShortener.Application.Features.Finance.Interfaces.Repositories;
using URLShortener.Domain.Entities.Finance;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories.Finance;

public sealed class PurchaseRepository(AppDbContext context) : GenericRepository<PurchaseModel>(context), IPurchaseRepository
{
}
