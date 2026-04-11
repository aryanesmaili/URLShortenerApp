using URLShortener.Domain.Entities.URL;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Application.Interfaces.Services.User;

public interface IAuthService
{
    string GenerateJWToken(string Username, string Role, string Email);
    Task AuthorizeURLsAccessAsync(int userID, string reqUsername);
    Task<UserModel> AuthorizeUserAccessAsync(int UserID, string reqUsername, bool includeRelations = false);
    Task<URLModel> AuthorizeURLAccessAsync(int urlID, string username, bool includeRelations = false);
    string GenerateRefreshToken();
    string GenerateRandomPassword(int length);
    Task<UserModel> AuthorizeUserDepositsAccess(int UserID, string reqUsername);
}
