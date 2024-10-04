using URLShortenerAPI.Data.Entities.User;

namespace URLShortenerAPI.Services.Interfaces
{
    public interface IUserService
    {
        public Task<UserDTO> GetUserByIDAsync(int id);
        public Task<UserDTO> GetFullUserInfo(int id);
        public Task<UserDTO> GetUserByUsernameAsync(string Username);
        public Task<UserDTO> RegisterUserAsync(UserCreateDTO newUserInfo);
        public Task<UserDTO> LoginUserAsync(UserLoginDTO user);
        public Task<UserDTO> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, string requestingUsername);
        public Task DeleteUser(int id);
        public Task<UserDTO> ResetPasswordAsync(string Identifier);
        public Task<UserDTO> ChangePasswordAsync(ChangePasswordRequest reqInfo, string requestingUsername);
        public Task<UserDTO> TokenRefresher(string refreshToken);
        public Task<UserDTO> CheckResetCode(UserDTO user, string Code);
        public Task RevokeToken(string token);
        public bool IsUser(int id);
        public bool IsUser(string username);
        public bool IsEmailTaken(string email);
        public bool IsAdmin(int id);
        public bool IsAdmin(string username);
    }
}
