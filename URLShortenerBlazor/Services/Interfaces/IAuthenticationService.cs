using SharedDataModels.DTOs;
using SharedDataModels.Responses;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task LogOutAsync();
        public Task<APIResponse<UserDTO>> Login(UserLoginDTO loginInfo);
        public Task<APIResponse<UserDTO>> Register(UserCreateDTO userCreateDTO);
        public Task<int> GetUserIDAsync();
        public Task RefreshTokenAsync();
        public Task<UserDTO> GetUserInfoAsync();
        public Task UpdateUserInfo(UserDTO user);
    }
}