using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Data.Entities.URLCategory;

namespace URLShortenerAPI.Data.Entities.User
{
    internal class UserModel
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public string? ResetCode { get; set; }
        public UserType Role { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<URLModel>? URLs { get; set; }
        public ICollection<URLCategoryModel>? URLCategories { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; }  
    }
    public class UserDTO
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public UserType Role { get; set; }

        public string? JWToken { get; set; }
        public RefreshTokenDTO? RefreshToken { get; set; }

        public List<URLDTO>? URLs { get; set; }
        public List<CategoryDTO>? Categories { get; set; } // owned categories
    }
    public class UserCreateDTO
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
    }
    public class UserUpdateDTO 
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
    }

    public class UserLoginDTO
    {
        public required string Identifier { get; set; } // could be email or password or phone number.
        public required string Password { get; set; }
    }
    public enum UserType
    {
        Admin,
        ChannelAdmin
    }

    public class ChangePasswordRequest
    {
        public required UserDTO UserInfo { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }

    internal class RefreshToken
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; }
        public bool IsActive => Revoked == null && !IsExpired;
        public bool IsExpired => DateTime.UtcNow >= Expires;

        public required int UserId { get; set; }
        public required UserModel User { get; set; }
    }
    public class RefreshTokenDTO
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
    }
}
