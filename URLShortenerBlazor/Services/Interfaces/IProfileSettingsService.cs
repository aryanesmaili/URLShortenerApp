using SharedDataModels.DTOs;
using SharedDataModels.Responses;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IProfileSettingsService
    {
        Task<APIResponse<UserDTO>> ChangeUserInfo(UserUpdateDTO userUpdate);
        Task<APIResponse<string>> RequestChangePassword(string identifier);
        Task<APIResponse<UserDTO>> SendPasswordVerificationCode(CheckVerificationCode reqInfo);
        Task<APIResponse<UserDTO>> SendChangePassowrdRequest(ChangePasswordRequest request);
        Task<APIResponse<string>> RequestChangingEmail(int userID);
        Task<APIResponse<string>> SendEmailVerificationCode(int userID, CheckVerificationCode reqInfo);
        Task<APIResponse<UserDTO>> SendChangeEmailRequest(int userID, ChangeEmailRequest reqInfo);
    }
}