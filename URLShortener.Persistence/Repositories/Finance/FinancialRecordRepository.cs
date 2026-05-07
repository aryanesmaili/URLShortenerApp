using URLShortener.Application.Features.Finance.Interfaces.Repositories;
using URLShortener.Domain.Entities.Finance;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories.Finance;

public sealed class FinancialRecordRepository(AppDbContext context) : GenericRepository<FinancialRecordModel>(context), IFinancialRecordRepository
{
}
