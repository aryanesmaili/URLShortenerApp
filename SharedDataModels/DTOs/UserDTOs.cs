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

    public class UserDashboardDTO
    {
        public Dictionary<string, int>? MonthlyChartData { get; set; }
        public Dictionary<string, int>? DailyChartData { get; set; }
        public List<string>? TopCountries { get; set; }
        public List<string>? TopDevices { get; set; }
        public List<URLDTO>? TopClickedURLs { get; set; }
        public List<URLDTO>? MostRecentURLs { get; set; }
    }

    public class UserCreateDTO
    {
        [Required(ErrorMessage = "Full Name is Required.")]
        [Length(5, 64, ErrorMessage = "Your Full Name should have at least 5 Characters and at most 64 Characters.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Email is Required.")]
        [EmailAddress(ErrorMessage = "Entered value is not a valid Email.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Username is Required.")]
        [Length(5, 32, ErrorMessage = "Your Username should have at least 5 Characters and at most 32 Characters.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is Required.")]
        [Length(5, 64, ErrorMessage = "Your Password should have at least 5 Characters and at most 64 Characters.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required.")]
        [Length(5, 64, ErrorMessage = "Your Confirm Password should have at least 5 Characters and at most 64 Characters.")]
        [Compare("Password", ErrorMessage = "Your Password is not equal to your Confirm Password.")]
        public required string ConfirmPassword { get; set; }
    }
    public class UserUpdateDTO
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        [EmailAddress(ErrorMessage = "Entered value is not a valid Email.")]
        public string? Email { get; set; }
        public string? Username { get; set; }
    }

    public class UserLoginDTO
    {
        [Required(ErrorMessage = "Email or Username is Required.")]
        public string? Identifier { get; set; } // could be email or Username.
        [Required(ErrorMessage = "Password is Required.")]
        [Length(5, 64, ErrorMessage = "Your Password should have at least 5 Characters and at most 64 Characters.")]
        public string? Password { get; set; }
    }

    public class UserLoginResponse
    {
        public required UserDTO User { get; set; }
        public required RefreshTokenDTO RefreshToken { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "A valid UserInfo is required.")]
        public required UserDTO UserInfo { get; set; }

        [Required(ErrorMessage = "Password is Required.")]
        [Length(5, 64, ErrorMessage = "Your Password should have at least 5 Characters and at most 64 Characters.")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required.")]
        [Length(5, 64, ErrorMessage = "Your Confirm Password should have at least 5 Characters and at most 64 Characters.")]
        [Compare("Password", ErrorMessage = "Your Password is not equal to your Confirm Password.")]
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

