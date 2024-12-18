﻿using SharedDataModels.DTOs;
using SharedDataModels.Responses;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task LogOutAsync(bool backendlogout = true);
        public Task<APIResponse<UserDTO>> Login(UserLoginDTO loginInfo);
        public Task<APIResponse<UserDTO>> Register(UserCreateDTO userCreateDTO);
        public Task<int> GetUserIDAsync();
        public Task<UserDTO> GetUserInfoAsync();
        public Task UpdateUserInfo(UserDTO user);
        public Task ClearUserInfo();
        Task<APIResponse<CaptchaVerificationResponse>> VerifyCaptcha(string token);
    }
}