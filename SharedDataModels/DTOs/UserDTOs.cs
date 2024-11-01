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
        public DateTime CreatedAt { get; set; }

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
        public Dictionary<string, int>? HourlyChartData { get; set; }
        public List<string>? TopCountries { get; set; }
        public List<string>? TopOSs { get; set; }
        public List<URLDTO>? TopClickedURLs { get; set; }
        public List<URLDTO>? MostRecentURLs { get; set; }
    }

    public class UserCreateDTO
    {
        private string? _name;

        [Required(ErrorMessage = "Full Name is Required.")]
        [StringLength(64, MinimumLength = 5, ErrorMessage = "Your Full Name should have at least 5 Characters and at most 64 Characters.")]
        public required string Name
        {
            get => _name ?? string.Empty;
            set => _name = value?.Trim();
        }

        private string? _email;

        [Required(ErrorMessage = "Email is Required.")]
        [EmailAddress(ErrorMessage = "Entered value is not a valid Email.")]
        public required string Email
        {
            get => _email ?? string.Empty;
            set => _email = value?.Trim();
        }

        private string? _username;

        [Required(ErrorMessage = "Username is Required.")]
        [StringLength(32, MinimumLength = 5, ErrorMessage = "Your Username should have at least 5 Characters and at most 32 Characters.")]
        public required string Username
        {
            get => _username ?? string.Empty;
            set => _username = value?.Trim();
        }

        private string? _password;

        [Required(ErrorMessage = "Password is Required.")]
        [StringLength(64, MinimumLength = 5, ErrorMessage = "Your Password should have at least 5 Characters and at most 64 Characters.")]
        public required string Password
        {
            get => _password ?? string.Empty;
            set => _password = value?.Trim();
        }

        private string? _confirmPassword;

        [Required(ErrorMessage = "Confirm Password is Required.")]
        [StringLength(64, MinimumLength = 5, ErrorMessage = "Your Confirm Password should have at least 5 Characters and at most 64 Characters.")]
        [Compare("Password", ErrorMessage = "Your Password is not equal to your Confirm Password.")]
        public required string ConfirmPassword
        {
            get => _confirmPassword ?? string.Empty;
            set => _confirmPassword = value?.Trim();
        }
    }

    public class UserUpdateDTO
    {
        public int ID { get; set; }

        private string? _name;
        public string? Name
        {
            get => _name;
            set => _name = value?.Trim();
        }

        private string? _username;
        public string? Username
        {
            get => _username;
            set => _username = value?.Trim();
        }
    }

    public class UserLoginDTO
    {
        private string? _identifier;

        [Required(ErrorMessage = "Email or Username is Required.")]
        public string? Identifier
        {
            get => _identifier;
            set => _identifier = value?.Trim();
        }

        private string? _password;

        [Required(ErrorMessage = "Password is Required.")]
        [StringLength(64, MinimumLength = 5, ErrorMessage = "Your Password should have at least 5 Characters and at most 64 Characters.")]
        public string? Password
        {
            get => _password;
            set => _password = value?.Trim();
        }
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

        private string? _newPassword;

        [Required(ErrorMessage = "Password is Required.")]
        [StringLength(64, MinimumLength = 5, ErrorMessage = "Your Password should have at least 5 Characters and at most 64 Characters.")]
        public required string NewPassword
        {
            get => _newPassword ?? string.Empty;
            set => _newPassword = value?.Trim();
        }

        private string? _confirmPassword;

        [Required(ErrorMessage = "Confirm Password is Required.")]
        [StringLength(64, MinimumLength = 5, ErrorMessage = "Your Confirm Password should have at least 5 Characters and at most 64 Characters.")]
        [Compare("NewPassword", ErrorMessage = "Your Password is not equal to your Confirm Password.")]
        public required string ConfirmPassword
        {
            get => _confirmPassword ?? string.Empty;
            set => _confirmPassword = value?.Trim();
        }
    }

    public class ChangeEmailRequest
    {
        private string? _code;

        public string Code
        {
            get => _code ?? string.Empty;
            set => _code = value?.Trim();
        }

        private string? _email;

        [EmailAddress(ErrorMessage = "Entered value is not a valid Email.")]
        public string NewEmail
        {
            get => _email ?? string.Empty; 
            set => _email = value?.Trim();
        }
    }
    public class RefreshTokenDTO
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
    }
}

