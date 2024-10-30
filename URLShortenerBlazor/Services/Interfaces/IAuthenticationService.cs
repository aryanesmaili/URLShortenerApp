using SharedDataModels.DTOs;
using SharedDataModels.Responses;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<bool> IsLoggedInAsync();
        public Task LogOutAsync();
        public Task<APIResponse<UserDTO>> Login(UserLoginDTO loginInfo);
        public Task<APIResponse<UserDTO>> Register(UserCreateDTO userCreateDTO);
        public Task<int> GetUserIDAsync();
        public Task<string?> RefreshTokenAsync();
        public Task<UserDTO?> GetUserInfoAsync();
    }
}