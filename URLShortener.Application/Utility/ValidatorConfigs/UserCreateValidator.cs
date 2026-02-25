using FluentValidation;
using URLShortener.Application.DTOs;

namespace URLShortenerAPI.Responses.ValidatorConfigs
{
    public class UserCreateValidator : AbstractValidator<UserCreateDTO>
    {
        public UserCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(4)
                .MaximumLength(32);

            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$").WithMessage("Invalid email address format.");

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
                .MinimumLength(5);
        }
    }
    public class UserUpdateValidator : AbstractValidator<UserUpdateDTO>
    {
        public UserUpdateValidator()
        {
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Name) || !string.IsNullOrWhiteSpace(x.Username))
                .WithMessage("At least one of Name or Username must be filled.");

            RuleFor(x => x.Name)
                .MinimumLength(4)
                .When(x => !string.IsNullOrWhiteSpace(x.Name))
                .WithMessage("Name must be at least 4 characters long.");
        }
    }
    public class UserChangeEmailValidator : AbstractValidator<ChangeEmailRequest>
    {
        public UserChangeEmailValidator()
        {
            RuleFor(x => x.NewEmail)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.NewEmail))
                .WithMessage("Invalid email format.");
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
        public ChangePasswordValidator()
        {
            RuleFor(x => x.UserInfo).NotEmpty();

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
