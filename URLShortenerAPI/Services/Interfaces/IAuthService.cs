using URLShortenerAPI.Data.Entites.URL;
using URLShortenerAPI.Data.Entites.User;

namespace URLShortenerAPI.Services.Interfaces
{
    internal interface IAuthService
    {
        public string GenerateJWToken(string Username, string Role, string Email);
        public Task<UserModel> AuthorizeUserAccessAsync(int UserID, string reqUsername);
        public Task<URLModel> AuthorizeURLAccessAsync(int urlID, string username)
        public string GenerateRefreshToken();
        public string GenerateRandomPassword(int length);

    }
}
