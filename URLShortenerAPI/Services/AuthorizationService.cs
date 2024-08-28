using Microsoft.EntityFrameworkCore;
using Pexita.Utility.Exceptions;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entites.User;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Services
{
    internal class AuthorizationService(AppDbContext context) : IAuthorizationService
    {
        private readonly AppDbContext _context = context;

        public async Task<UserModel> AuthorizeUserAccessAsync(int UserID, string reqUsername)
        {
            if (string.IsNullOrEmpty(reqUsername))
            {
                throw new ArgumentNullException(nameof(reqUsername));
            }

            UserModel user = await _context.Users
                .Include(u => u.URLs)
                .Include(u => u.URLCategories)
                .FirstOrDefaultAsync(x => x.ID == UserID) 
                ?? throw new NotFoundException($"User {UserID} not found.");

            UserModel reqUser = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == reqUsername) ?? throw new NotFoundException($"User {UserID} not found."); ;

            bool isAdmin = reqUser.Role == UserType.Admin;
            bool isOwner = reqUser.ID == user.ID;

            if (!isAdmin && !isOwner)
            {
                throw new NotAuthorizedException($"User {reqUsername} is not Authorized to modify User {UserID}");
            }
            return user;
        }
    }
}
