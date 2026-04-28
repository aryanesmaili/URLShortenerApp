using SharedDataModels.Responses;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.Models;

namespace URLShortener.Application.Interfaces.Services.User;

public interface IAuthenticationService
{
    Task ConfirmEmailChangeAsync(long userId, string newEmail, string encodedToken);
    Task<UserLoginResponse> LoginAsync(UserLoginDTO loginInfo);
    Task<UserDTO> RegisterUserAsync(UserCreateDTO newUserInfo);
    Task RequestEmailChangeAsync(long userId, string newEmail);
    Task RequestPasswordResetAsync(ChangePasswordRequest request);
    Task ResetPasswordAsync(string email, string encodedToken, string newPassword);
    Task RevokeTokenAsync(string token);
    void SoftDeleteIdentityUser(AppIdentityUser user);
    Task<(string jwt, string refreshToken)> TokenRefresher(string refreshToken);
    Task<CaptchaVerificationResponse> VerifyCaptcha(string token, string userIP);
}
