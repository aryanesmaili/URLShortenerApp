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
        public string? Identifier { get; set; } // could be email or password or phone number.
        public string? Password { get; set; }
    }

    public class UserLoginResponse
    {
        public required UserDTO User { get; set; }
        public required RefreshTokenDTO RefreshToken { get; set; }
    }

    public class ChangePasswordRequest
    {
        public required UserDTO UserInfo { get; set; }
        public required string NewPassword { get; set; }
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

