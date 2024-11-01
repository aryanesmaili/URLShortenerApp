using SharedDataModels.DTOs;
using SharedDataModels.Responses;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IProfileSettingsService
    {
        Task<APIResponse<UserDTO>> ChangePassowrd(ChangePasswordRequest request);
        Task<APIResponse<UserDTO>> ChangeUserInfo(UserUpdateDTO userUpdate);
        Task<APIResponse<string>> RequestChangingEmail(int userID);
        Task<APIResponse<UserDTO>> SendChangeEmailRequest(int userID, ChangeEmailRequest reqInfo);
        Task<APIResponse<string>> SendEmailChangeResetCode(int userID, ChangeEmailRequest reqInfo);
    }
}