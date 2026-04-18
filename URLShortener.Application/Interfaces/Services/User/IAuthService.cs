using URLShortener.Domain.Entities.URL;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Application.Interfaces.Services.User;

public interface IAuthService
{
    Task AuthorizeURLsAccessAsync(int userID, string reqUsername);
    Task<UserModel> AuthorizeUserAccessAsync(int UserID, string reqUsername, bool includeRelations = false);
    Task<URLModel> AuthorizeURLAccessAsync(int urlID, string username, bool includeRelations = false);
    Task<UserModel> AuthorizeUserDepositsAccess(int UserID, string reqUsername);
}
