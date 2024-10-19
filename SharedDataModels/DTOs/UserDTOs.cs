using System.ComponentModel.DataAnnotations;

namespace SharedDataModels.DTOs
{
    public class UserDTO
    {
        public int ID { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public UserType Role { get; set; }

        public string? JWToken { get; set; }

        public List<URLDTO>? URLs { get; set; }
        public List<CategoryDTO>? Categories { get; set; } // owned categories
    }
    public enum UserType
    {
        Admin,
        ChannelAdmin
    }

    public class UserCreateDTO
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [Length(5, 32)]
        public required string Username { get; set; }
        [Required]
        [Length(5, 64)]
        public required string Password { get; set; }
        [Required]
        [Length(5, 64)]
        public required string ConfirmPassword { get; set; }
    }
    public class UserUpdateDTO
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? Username { get; set; }
    }

    public class UserLoginDTO
    {
        [Required]
        public string? Identifier { get; set; } // could be email or password or phone number.
        [Required]
        public string? Password { get; set; }
    }

    public class UserLoginResponse
    {
        public required UserDTO User { get; set; }
        public required RefreshTokenDTO RefreshToken { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public required UserDTO UserInfo { get; set; }
        [Required]
        [Length(5, 64)]
        public required string NewPassword { get; set; }
        [Required]
        [Length(5, 64)]
        public required string ConfirmPassword { get; set; }
    }
    public class RefreshTokenDTO
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
    }

}

