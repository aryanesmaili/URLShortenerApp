using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Data.Entities.User;

namespace URLShortenerAPI.Services.Interfaces.UserRelated
{
    internal interface IAuthService
    {
        public string GenerateJWToken(string Username, string Role, string Email);
        public Task AuthorizeURLsAccessAsync(int userID, string reqUsername);
        public Task<UserModel> AuthorizeUserAccessAsync(int UserID, string reqUsername, bool includeRelations = false);
        public Task<URLModel> AuthorizeURLAccessAsync(int urlID, string username, bool includeRelations = false);
        public string GenerateRefreshToken();
        public string GenerateRandomPassword(int length);

    }
}
