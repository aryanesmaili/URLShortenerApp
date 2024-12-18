﻿using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedDataModels.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entities.Finance;
using URLShortenerAPI.Data.Entities.Settings;
using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Data.Entities.User;
using URLShortenerAPI.Data.Interfaces.User;
using URLShortenerAPI.Utility.Exceptions;

namespace URLShortenerAPI.Services.User
{
    internal class AuthService(AppDbContext context, JwtSettings jwtSettings) : IAuthService
    {
        private readonly AppDbContext _context = context;
        private readonly JwtSettings _jwtSettings = jwtSettings;

        /// <summary>
        /// Authorizes whether a given username has authority to access another user's urls.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="reqUsername"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="NotAuthorizedException"></exception>
        public async Task AuthorizeURLsAccessAsync(int userID, string reqUsername)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reqUsername);

            UserModel reqUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Username == reqUsername)
                ?? throw new NotFoundException($"user {reqUsername} Does not Exist");

            if (!await _context.Users.AnyAsync(x => x.ID == userID))
                throw new NotFoundException($"user {userID} Does not Exist");

            else if (reqUser.ID != userID)
                throw new NotAuthorizedException($"User {reqUsername} cannot access user {userID}'s URLs.");
        }

        /// <summary>
        /// Authorizes whether a given username has authority to access a url.
        /// </summary>
        /// <param name="urlID"></param>
        /// <param name="username"></param>
        /// <returns>a <see cref="URLModel"/> object to modify.</returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="NotAuthorizedException"></exception>
        public async Task<URLModel> AuthorizeURLAccessAsync(int urlID, string username, bool includeRelations = false)
        {
            ArgumentException.ThrowIfNullOrEmpty(username);
            URLModel url;
            if (includeRelations)
                url = await _context.URLs
                    .Include(x => x.User)
                    .Include(x => x.Clicks)
                    .Include(x => x.Categories)
                    .Include(x => x.URLAnalytics)
                    .FirstOrDefaultAsync(x => x.ID == urlID)
                    ?? throw new NotFoundException($"URL {urlID} Does not exist.");
            else
                url = await _context.URLs
                     .FirstOrDefaultAsync(x => x.ID == urlID)
                     ?? throw new NotFoundException($"URL {urlID} Does not exist.");

            UserModel reqUser = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Username == username) ?? throw new NotFoundException($"User {username} not found.");

            bool isAdmin = reqUser.Role == UserType.Admin;
            bool isOwner = reqUser.ID == url.UserID;

            if (!isAdmin && !isOwner)
            {
                throw new NotAuthorizedException($"User {username} is not Authorized to access url {urlID}");
            }
            return url;
        }

        /// <summary>
        /// Authorizes whether a given username has authority to access a user's Deposits.
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="reqUsername"></param>
        /// <returns>the List of <see cref="DepositModel"/>s to be Accessed.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="NotAuthorizedException"></exception>
        public async Task<UserModel> AuthorizeUserDepositsAccess(int UserID, string reqUsername)
        {
            if (string.IsNullOrEmpty(reqUsername))
                throw new ArgumentNullException(nameof(reqUsername));

            UserModel user = await _context.Users.Include(x => x.FinancialRecord)
                                                 .ThenInclude(x => x.Deposits)
                                                 .FirstOrDefaultAsync(x => x.ID == UserID)
                                                  ?? throw new NotFoundException($"User {UserID} not found.");

            UserModel reqUser = await _context.Users.AsNoTracking()
                                                    .FirstOrDefaultAsync(x => x.Username == reqUsername)
                                                    ?? throw new NotFoundException($"User {reqUsername} not found.");

            bool isAdmin = reqUser.Role == UserType.Admin;
            bool isOwner = reqUser.ID == user.ID;

            if (!isAdmin && !isOwner)
            {
                throw new NotAuthorizedException($"User {reqUsername} is not Authorized to Access User {UserID}'s records");
            }

            return user;
        }

        /// <summary>
        /// Authorizes whether a given username has authority to access a user's data.
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="reqUsername"></param>
        /// <returns>the <see cref="UserModel"/> to be modified.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="NotAuthorizedException"></exception>
        public async Task<UserModel> AuthorizeUserAccessAsync(int UserID, string reqUsername, bool includeRelations)
        {
            if (string.IsNullOrEmpty(reqUsername))
                throw new ArgumentNullException(nameof(reqUsername));

            UserModel user;
            if (includeRelations)
                user = await _context.Users
                   .Include(u => u.URLs)
                   .Include(u => u.URLCategories)
                   .Include(u => u.FinancialRecord)
                   .FirstOrDefaultAsync(x => x.ID == UserID)
                   ?? throw new NotFoundException($"User {UserID} not found.");
            else
                user = await _context.Users
                   .FirstOrDefaultAsync(x => x.ID == UserID)
                   ?? throw new NotFoundException($"User {UserID} not found.");

            UserModel reqUser = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == reqUsername) ?? throw new NotFoundException($"User {reqUsername} not found.");

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
        /// <summary>
        /// Generates a random password containing alphabet characters and numbers.
        /// </summary>
        /// <param name="length">length of password.</param>
        /// <returns></returns>
        public string GenerateRandomPassword(int length = 8)
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
