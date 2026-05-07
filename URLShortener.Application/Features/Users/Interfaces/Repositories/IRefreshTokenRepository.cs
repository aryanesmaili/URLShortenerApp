using URLShortener.Application.Common.Interfaces.Repositories;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Application.Features.Users.Interfaces.Repositories;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
}
