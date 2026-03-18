using SharedDataModels.Responses;
using URLShortener.Application.DTOs.EntityDTOs.URL;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Common.Responses;

namespace URLShortener.Application.Interfaces.Services.User
{
    public interface IUserService
    {
        Task<UserDTO> GetUserByIDAsync(int id);
        Task<UserDTO> GetFullUserInfoAsync(int id);
        Task<UserDTO> GetUserByUsernameAsync(string Username);
        Task<UserDashboardDTO> GetDashboardByIDAsync(int userID, string reqUsername);
        Task<UserDTO> RegisterUserAsync(UserCreateDTO newUserInfo);
        Task<UserLoginResponse> LoginUserAsync(UserLoginDTO user);
        Task<UserLoginResponse> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, string requestingUsername);
        Task<double> GetUserBalance(int userID, string username);
        Task<UserStats> GetUserStats(int userID, string username);
        Task<PagedResult<URLDTO>> GetPagedResult(int userID, int pageNumber, int pageSize, string reqUsername);
        Task DeleteUserAsync(int id);
        Task ResetPasswordAsync(string Identifier);
        Task<UserDTO> ChangePasswordAsync(ChangePasswordRequest reqInfo, string requestingUsername);
        Task<string> TokenRefresher(string refreshToken);
        Task<UserLoginResponse> CheckPasswordResetCodeAsync(string identifier, string Code);
        Task ResetEmailAsync(int userID, string reqUsername);
        Task CheckEmailResetCodeAsync(string code, int userID, string reqUsername);
        Task<UserDTO> SetNewEmailAsync(string newEmail, int userID, string reqUsername);
        Task RevokeTokenAsync(string token);
        Task<bool> IsUser(int id);
        Task<bool> IsUser(string username);
        Task<bool> IsEmailTaken(string email);
        Task<bool> IsAdmin(int id);
        Task<bool> IsAdmin(string username);
        Task<CaptchaVerificationResponse> VerifyCaptcha(string token, string userIP);
    }
}
