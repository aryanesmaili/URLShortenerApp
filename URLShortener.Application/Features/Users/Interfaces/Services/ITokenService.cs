using URLShortener.Application.Common.Models.Identity;
using URLShortener.Application.Features.Users.DTOs;

namespace URLShortener.Application.Features.Users.Interfaces.Services;

public interface ITokenService
{
    Task<string> GenerateJWTokenAsync(AppIdentityUser identityUser);
    Task<RefreshTokenDTO> GenerateRefreshTokenAsync(long identityUserId);
    Task<(string jwt, string refreshToken)> RefreshAsync(string refreshToken);
    Task RevokeAllUserRefreshTokens(long userId);
    Task RevokeTokenAsync(string token);
}
