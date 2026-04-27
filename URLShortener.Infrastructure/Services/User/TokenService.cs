using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Interfaces.Services.User;
using URLShortener.Application.Models;
using URLShortener.Application.Repositories;
using URLShortener.Application.Utility.Exceptions;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Infrastructure.Services.User;

/// <summary>
/// Handles JWT generation and refresh token lifecycle (issue, rotate, revoke).
/// </summary>
/// <remarks>
/// Raw refresh tokens are never stored — only their SHA-256 hashes are persisted.
/// Rotation is atomic (wrapped in a DB transaction). If a revoked token is reused,
/// all active tokens for that user are immediately revoked as a breach mitigation.
///
/// Intended as a scoped/per-request service — not safe to share across requests.
/// </remarks>
public sealed class TokenService(
    UserManager<AppIdentityUser> userManager,
    IOptions<JwtSettings> jwtSettings,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork uow,
    IMapper mapper) : ITokenService
{
    private readonly UserManager<AppIdentityUser> _userManager = userManager;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IUnitOfWork _uow = uow;
    private readonly IMapper _mapper = mapper;

    // ================= JWT =================

    /// <summary>
    /// Builds a JWT for the given user, pulling their roles and custom claims from the UserManager.
    /// </summary>
    public async Task<string> GenerateJWTokenAsync(AppIdentityUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!)
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var userClaims = await _userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        return GenerateJWToken(claims);
    }

    /// <summary>
    /// Signs a JWT from the given claims using the symmetric key and settings from <see cref="JwtSettings"/>.
    /// </summary>
    private string GenerateJWToken(List<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.TokenSecretKey);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        var token = tokenHandler.CreateToken(descriptor);
        return tokenHandler.WriteToken(token);
    }

    // ================= REFRESH TOKEN =================

    /// <summary>
    /// Issues a new refresh token for the given user. The raw token is returned to the caller
    /// (to be stored in an httpOnly cookie); only the SHA-256 hash is persisted.
    /// </summary>
    public async Task<RefreshTokenDTO> GenerateRefreshTokenAsync(long userId)
    {
        var rawToken = GenerateSecureToken();
        var hashed = HashRefreshToken(rawToken);
        var now = DateTime.UtcNow;

        var entity = new RefreshToken
        {
            TokenHash = hashed,
            Created = now,
            Expires = now.AddDays(7),
            IdentityUserId = userId,
            Revoked = null
        };

        _refreshTokenRepository.Add(entity);
        await _uow.SaveChangesAsync();

        return _mapper.Map<RefreshTokenDTO>(entity);
    }

    /// <summary>
    /// Validates the provided refresh token, revokes it, and returns a fresh JWT + refresh token pair.
    /// The entire rotate operation runs inside a transaction so there's no window where both tokens are valid.
    /// </summary>
    /// <exception cref="ArgumentNullException">Token string was null or empty.</exception>
    /// <exception cref="NotFoundException">No record matched the token hash.</exception>
    /// <exception cref="SecurityException">Token was already revoked — reuse detected, all user tokens nuked.</exception>
    /// <exception cref="RefreshTokenExpiredException">Token exists but has expired.</exception>
    public async Task<(string jwt, string refreshToken)> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentNullException(nameof(refreshToken));

        var hashed = HashRefreshToken(refreshToken);

        using var tx = await _uow.BeginTransactionAsync();

        var token = await _refreshTokenRepository
            .GetAsync(t => t.TokenHash == hashed)
            ?? throw new NotFoundException(nameof(RefreshToken), nameof(RefreshToken.TokenHash), hashed[..8]);

        // Reuse detection: a non-null Revoked means someone is replaying an old token.
        // Revoke everything for this user and bail — treat it as a potential compromise.
        if (token.Revoked != null)
        {
            await RevokeAllUserRefreshTokens(token.IdentityUserId);
            throw new SecurityException("Token reuse detected");
        }

        if (!token.IsActive)
            throw new RefreshTokenExpiredException("Token is not active");

        var user = await _userManager.FindByIdAsync(token.IdentityUserId.ToString())
            ?? throw new NotFoundException(nameof(AppIdentityUser), nameof(AppIdentityUser.Id), token.IdentityUserId);

        // Revoke the old token and issue a replacement.
        token.Revoked = DateTime.UtcNow;

        var newRawToken = GenerateSecureToken();
        var newToken = new RefreshToken
        {
            TokenHash = HashRefreshToken(newRawToken),
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(7),
            IdentityUserId = user.Id
        };

        _refreshTokenRepository.Add(newToken);

        var jwt = await GenerateJWTokenAsync(user);

        await _uow.SaveChangesAsync();
        await tx.CommitAsync();

        return (jwt, newRawToken);
    }

    /// <summary>
    /// Revokes a refresh token. Safe to call on tokens that are already inactive or don't exist.
    /// </summary>
    public async Task RevokeTokenAsync(string refreshToken)
    {
        var hashed = HashRefreshToken(refreshToken);

        var token = await _refreshTokenRepository
            .GetAsync(t => t.TokenHash == hashed);

        if (token == null || !token.IsActive)
            return;

        token.Revoked = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
    }

    /// <summary>
    /// Finds and revokes all non-revoked tokens for a user.
    /// Called when token reuse is detected to limit the blast radius of a potential theft.
    /// </summary>
    public async Task RevokeAllUserRefreshTokens(long userId)
    {
        var tokens = await _refreshTokenRepository
            .GetListAsync(t => t.IdentityUserId == userId && t.Revoked == null);

        var now = DateTime.UtcNow;
        foreach (var t in tokens)
            t.Revoked = now;

        await _uow.SaveChangesAsync();
    }

    // ================= HELPERS =================

    /// <summary>
    /// Generates a cryptographically secure random token (64 bytes, Base64-encoded).
    /// </summary>
    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Returns the SHA-256 hash of the token, Base64-encoded.
    /// This is what gets stored in the DB — never the raw token.
    /// </summary>
    private static string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(bytes);
    }
}
