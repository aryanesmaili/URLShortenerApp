using URLShortener.Application.Features.Users.Interfaces.Repositories;
using URLShortener.Domain.Entities.User;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories.Users;

public sealed class RefreshTokenRepository(AppDbContext context) : GenericRepository<RefreshToken>(context), IRefreshTokenRepository
{
}
