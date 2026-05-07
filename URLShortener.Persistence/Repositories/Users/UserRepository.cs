using System.Collections.Generic;
using URLShortener.Application.Features.Users.Interfaces.Repositories;
using URLShortener.Domain.Entities.User;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories.Users;

public sealed class UserRepository(AppDbContext context) : GenericRepository<UserModel>(context), IUserRepository
{
    private readonly AppDbContext _context = context;

    public override void Remove(UserModel userModel)
    {
        userModel.SoftDelete();
    }

    public override void RemoveRange(IEnumerable<UserModel> entities)
    {
        foreach (var entity in entities)
            entity.SoftDelete();
    }
}
