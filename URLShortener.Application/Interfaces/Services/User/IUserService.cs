using SharedDataModels.Responses;
using URLShortener.Application.DTOs.EntityDTOs.User;

namespace URLShortener.Application.Interfaces.Services.User;

public interface IUserService
{
    Task<UserDTO> GetUserByIDAsync(long userId);
    Task<UserDTO> RegisterUserAsync(UserCreateDTO newUserInfo);
    Task<UserLoginResponse> LoginUserAsync(UserLoginDTO user);
    Task<UserLoginResponse> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, string requestingUsername);
    Task DeleteUserAsync(int userId);
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
