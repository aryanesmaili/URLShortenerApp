﻿using SharedDataModels.CustomClasses;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;

namespace URLShortenerAPI.Data.Interfaces.User
{
    public interface IUserService
    {
        public Task<UserDTO> GetUserByIDAsync(int id);
        public Task<UserDTO> GetFullUserInfoAsync(int id);
        public Task<UserDTO> GetUserByUsernameAsync(string Username);
        public Task<UserDashboardDTO> GetDashboardByIDAsync(int userID, string reqUsername);
        public Task<UserDTO> RegisterUserAsync(UserCreateDTO newUserInfo);
        public Task<UserLoginResponse> LoginUserAsync(UserLoginDTO user);
        public Task<UserLoginResponse> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, string requestingUsername);
        public Task<double> GetUserBalance(int userID, string username);
        public Task<UserStats> GetUserStats(int userID, string username);
        public Task<PagedResult<URLDTO>> GetPagedResult(int userID, int pageNumber, int pageSize, string reqUsername);
        public Task DeleteUserAsync(int id);
        public Task ResetPasswordAsync(string Identifier);
        public Task<UserDTO> ChangePasswordAsync(ChangePasswordRequest reqInfo, string requestingUsername);
        public Task<string> TokenRefresher(string refreshToken);
        public Task<UserLoginResponse> CheckPasswordResetCodeAsync(string identifier, string Code);
        public Task ResetEmailAsync(int userID, string reqUsername);
        public Task CheckEmailResetCodeAsync(string code, int userID, string reqUsername);
        public Task<UserDTO> SetNewEmailAsync(string newEmail, int userID, string reqUsername);
        public Task RevokeTokenAsync(string token);
        public bool IsUser(int id);
        public bool IsUser(string username);
        public bool IsEmailTaken(string email);
        public bool IsAdmin(int id);
        public bool IsAdmin(string username);
        Task<CaptchaVerificationResponse> VerifyCaptcha(string token, string userIP);
    }
}
