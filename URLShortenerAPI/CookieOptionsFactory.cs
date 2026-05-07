using URLShortener.Application.Configuration;

namespace URLShortenerAPI;

internal static class CookieOptionsFactory
{
    public static CookieOptions CreateJwtCookieOptions(AuthenticationCookieSettings cookieSettings, JwtSettings jwtSettings)
    {
        return new CookieOptions
        {
            // Prevents access from JavaScript
            HttpOnly = cookieSettings.HttpOnly,
            // Set expiry for refresh token
            Expires = DateTimeOffset.UtcNow.AddMinutes(jwtSettings.ExpiresInMinutes),
            SameSite = ParseSameSite(cookieSettings.JwtSameSite),
            Secure = cookieSettings.Secure,
            Path = cookieSettings.Path
        };
    }

    public static CookieOptions CreateRefreshCookieOptions(AuthenticationCookieSettings cookieSettings)
    {
        return new CookieOptions
        {
            // Prevents access from JavaScript
            HttpOnly = cookieSettings.HttpOnly,
            // Set expiry for refresh token
            Expires = DateTimeOffset.UtcNow.AddDays(cookieSettings.RefreshTokenExpirationDays),
            // Prevents CSRF attacks
            SameSite = ParseSameSite(cookieSettings.RefreshTokenSameSite),
            Secure = cookieSettings.Secure,
            Path = cookieSettings.Path
        };
    }

    public static CookieOptions CreateDeletionOptions(AuthenticationCookieSettings cookieSettings)
    {
        return new CookieOptions
        {
            Path = cookieSettings.Path,
            Secure = cookieSettings.Secure,
            HttpOnly = cookieSettings.HttpOnly,
            SameSite = ParseSameSite(cookieSettings.RefreshTokenSameSite)
        };
    }

    private static SameSiteMode ParseSameSite(string sameSiteMode)
    {
        return Enum.Parse<SameSiteMode>(sameSiteMode, ignoreCase: true);
    }
}
