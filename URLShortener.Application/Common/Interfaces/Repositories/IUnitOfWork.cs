using Microsoft.EntityFrameworkCore.Storage;

namespace URLShortener.Application.Common.Interfaces.Repositories;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<int> SaveChangesAsync();
}
