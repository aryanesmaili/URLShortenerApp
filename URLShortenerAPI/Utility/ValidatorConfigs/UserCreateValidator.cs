using FluentValidation;
using SharedDataModels.DTOs;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Responses.ValidatorConfigs
{
    public class UserCreateValidator : AbstractValidator<UserCreateDTO>
    {
        private readonly IUserService _userService;
        public UserCreateValidator(IUserService userService)
        {
            _userService = userService;

            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(4)
                .MaximumLength(32);

            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$").WithMessage("Invalid email address format.")
                .Must(x => !_userService.IsEmailTaken(x)).WithMessage("Email Taken");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(5)
                .Equal(x => x.ConfirmPassword);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .MinimumLength(5)
                .Equal(x => x.Password);

            RuleFor(x => x.Username)
                .NotEmpty()
                .MinimumLength(5)
                .Must(x => !_userService.IsUser(x)).WithMessage($"Username Taken.");
        }
    }
    public class UserUpdateValidator : AbstractValidator<UserUpdateDTO>
    {
        private readonly IUserService _userService;

        public UserUpdateValidator(IUserService userService)
        {
            _userService = userService;
            RuleFor(x => x.Name).MinimumLength(4).When(x => x.Name?.Length > 0);

            RuleFor(x => x.Username).Must(x => !_userService.IsUser(x)).When(x => x.Username?.Length > 0);
        }
    }
    public class UserChangeEmailValidator : AbstractValidator<ChangeEmailRequest>
    {
        private readonly IUserService _userService;
        public UserChangeEmailValidator(IUserService userService)
        {
            _userService = userService;

            RuleFor(x => x.NewEmail)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.NewEmail))
                .WithMessage("Invalid email format.")
                .Must(email => !_userService.IsEmailTaken(email)).When(x => !string.IsNullOrEmpty(x.NewEmail))
                .WithMessage("Email is already taken.");
        }
    }
    public class UserLoginDTOValidator : AbstractValidator<UserLoginDTO>
    {
        public UserLoginDTOValidator()
        {
            RuleFor(x => x.Identifier).NotEmpty();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(5);
        }
    }
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
    {
        private readonly IUserService _userService;
        public ChangePasswordValidator(IUserService userService)
        {
            _userService = userService;
            RuleFor(x => x.UserInfo).NotEmpty().Must(x => _userService.IsUser(x.ID)).WithMessage($"Invalid User");

            RuleFor(x => x.NewPassword).NotEmpty()
                .MinimumLength(5).WithMessage("Your Password should have at least 5 Characters")
                .MaximumLength(64).WithMessage("Your Password should have at most 64 Characters");

            RuleFor(x => x.ConfirmPassword).NotEmpty()
                .MinimumLength(5).WithMessage("Your Password should have at least 5 Characters")
                .MaximumLength(64).WithMessage("Your Password should have at most 64 Characters")
                .Equal(x => x.NewPassword).WithMessage("entered Passowrd fields are not the same.");
        }
    }
}
