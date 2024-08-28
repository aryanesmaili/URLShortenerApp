using URLShortenerAPI.Data.Entites.User;

namespace URLShortenerAPI.Services.Interfaces
{
    internal interface IUserService
    {
        public Task<UserDTO> GetUserByIDAsync(int id);
        public Task<UserDTO> GetUserByUsernameAsync(string Username);
        public Task<UserDTO> RegsiterUserAsync(UserCreateDTO newUserInfo);
        public Task<UserDTO> LoginUserAsync(UserLoginDTO user);
        public Task<UserDTO> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, string requestingUsername);
        public Task DeleteUser(int id);
        public Task<UserDTO> ResetPasswordAsync(string Identifier);
        public Task<UserDTO> ChangePasswordAsync(UserDTO user, string newPassword, string ConfirmPassword, string requestingUsername);
        public Task<UserDTO> TokenRefresher(string refreshToken);
        public UserDTO UserModelToDTO(UserModel user);
        //public UserDTO UserModelToDTO(UserModel user, RefreshToken refreshToken, string AccessToken);
        public Task RevokeToken(string token);
        public bool IsUser(int id);
        public bool IsUser(string username);
        public bool IsEmailTaken(string email);
        public bool IsAdmin(int id);
        public bool IsAdmin(string username);
    }
}
