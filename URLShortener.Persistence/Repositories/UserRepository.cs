using URLShortener.Application.Repositories;
using URLShortener.Domain.Entities.User;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories;

public sealed class UserRepository : GenericRepository<UserModel>, IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
}
