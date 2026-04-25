using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.Models;

namespace URLShortener.Application.Interfaces.Services.User;

public interface ITokenService
{
    Task<string> GenerateJWTokenAsync(AppIdentityUser identityUser);
    Task<RefreshTokenDTO> GenerateRefreshTokenAsync(long identityUserId);
    Task<(string jwt, string refreshToken)> RefreshAsync(string refreshToken);
    Task RevokeAllUserRefreshTokens(long userId);
    Task RevokeTokenAsync(string token);
}
