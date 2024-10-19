using FluentValidation;
using SharedDataModels.DTOs;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Utility.ValidatorConfigs
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
                .EmailAddress()
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
            RuleFor(x => x.Name).MinimumLength(4).When(x => x.Name.Length > 0);

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Invalid email format.")
                .Must(email => !_userService.IsEmailTaken(email)).When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Email is already taken.");

            RuleFor(x => x.Username).Must(x => !_userService.IsUser(x)).When(x => x.Username.Length > 0);
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
}
