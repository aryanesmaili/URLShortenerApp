using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pexita.Utility.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entites.Settings;
using URLShortenerAPI.Data.Entites.User;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Services
{
    internal class AuthService(AppDbContext context, JwtSettings jwtSettings) : IAuthService
    {
        private readonly AppDbContext _context = context;
        private readonly JwtSettings _jwtSettings = jwtSettings;

        public async Task<UserModel> AuthorizeUserAccessAsync(int UserID, string reqUsername)
        {
            if (string.IsNullOrEmpty(reqUsername))
            {
                throw new ArgumentNullException(nameof(reqUsername));
            }

            UserModel user = await _context.Users
                .Include(u => u.URLs)
                .Include(u => u.URLCategories)
                .FirstOrDefaultAsync(x => x.ID == UserID)
                ?? throw new NotFoundException($"User {UserID} not found.");

            UserModel reqUser = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == reqUsername) ?? throw new NotFoundException($"User {UserID} not found."); ;

            bool isAdmin = reqUser.Role == UserType.Admin;
            bool isOwner = reqUser.ID == user.ID;

            if (!isAdmin && !isOwner)
            {
                throw new NotAuthorizedException($"User {reqUsername} is not Authorized to modify User {UserID}");
            }
            return user;
        }

        /// <summary>
        /// Generates a JWToken for a user based on their creds
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Role"></param>
        /// <param name="Email"></param>
        /// <returns> a string containing JWT token</returns>
        public string GenerateJWToken(string Username, string Role, string Email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, Username),
                    new Claim(ClaimTypes.Role, Role),
                    new Claim(ClaimTypes.Email, Email),
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRandomPassword(int length)
        {
            Random random = new();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder passwordBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int randomIndex = random.Next(0, chars.Length);
                char randomChar = chars[randomIndex];
                passwordBuilder.Append(randomChar);
            }

            return passwordBuilder.ToString();
        }

        /// <summary>
        /// Generates the refresh token needed for user to refresh their JWToken.
        /// </summary>
        /// <returns>a random string containing the new RefreshToken</returns>
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
    }
}
