using SharedDataModels.Responses;
using URLShortener.Application.Common.Models.Identity;
using URLShortener.Application.Features.Users.DTOs;

namespace URLShortener.Application.Features.Users.Interfaces.Services;

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
