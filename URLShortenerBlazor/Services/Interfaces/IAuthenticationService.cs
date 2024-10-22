using SharedDataModels.DTOs;
using SharedDataModels.Responses;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<bool> IsLoggedInAsync();
        public Task LogOut();
        public Task<APIResponse<UserDTO>> Login(UserLoginDTO loginInfo);
        public Task<APIResponse<UserDTO>> Register(UserCreateDTO userCreateDTO);
        public Task<int> GetUserID();
    }
}