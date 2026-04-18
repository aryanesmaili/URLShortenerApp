using SharedDataModels.Responses;
using URLShortener.Application.DTOs.EntityDTOs.User;

namespace URLShortener.Application.Interfaces.Services.User;

public interface IAuthenticationService
{
    Task<UserDTO> ChangePasswordAsync(ChangePasswordRequest reqInfo, string requestingUsername);
    Task CheckEmailResetCodeAsync(string code, int userID, string reqUsername);
    Task<UserLoginResponse> CheckPasswordResetCodeAsync(string identifier, string Code);
    string GenerateJWToken(string Username, string Role, string Email);
    string GenerateRandomPassword(int length = 8);
    string GenerateRefreshToken();
    Task<UserLoginResponse> LoginUserAsync(UserLoginDTO info);
    Task<UserDTO> RegisterUserAsync(UserCreateDTO newUserInfo);
    Task ResetEmailAsync(int userID, string reqUsername);
    Task ResetPasswordAsync(string identifier);
    Task RevokeTokenAsync(string token);
    Task<string> TokenRefresher(string refreshToken);
    Task<CaptchaVerificationResponse> VerifyCaptcha(string token, string userIP);
}
