using SharedDataModels.Responses;
using URLShortener.Application.DTOs.EntityDTOs.User;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task LogOutAsync(bool backendlogout = true);
        Task<APIResponse<UserDTO>> Login(UserLoginDTO loginInfo);
        Task<APIResponse<UserDTO>> Register(UserCreateDTO userCreateDTO);
        Task<int> GetUserIDAsync();
        Task<UserDTO> GetUserInfoAsync();
        Task UpdateUserInfo(UserDTO user);
        Task ClearUserInfo();
        Task<APIResponse<CaptchaVerificationResponse>> VerifyCaptcha(string token);
    }
}