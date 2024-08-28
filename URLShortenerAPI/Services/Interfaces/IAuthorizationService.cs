using URLShortenerAPI.Data.Entites.User;

namespace URLShortenerAPI.Services.Interfaces
{
    internal interface IAuthorizationService
    {
        public Task<UserModel> AuthorizeUserAccessAsync(int UserID, string reqUsername);

    }
}
