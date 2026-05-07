using FluentValidation;
using URLShortener.Application.Features.Users.DTOs;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortener.Application.Features.Users.Validation;

public class UserLoginDTOValidator : AbstractValidator<UserLoginDTO>
{
    public UserLoginDTOValidator()
    {
        // is a mandatory field that is first checked against being an email, if not, it's checked to be a ValidUsername 
        RuleFor(x => x.Identifier)
            .Cascade(CascadeMode.Stop)
            .EmailAddress()
            .WithMessage("Identifier must be a valid email address.")
            .ValidUsername(isRequired: true)
            .WithMessage("Identifier must be a valid username.");

        // is a mandatory field that has to be between 8-64 characters, and should contain at least one uppercase letter, one lowercase letter, one digit, and one special character
        RuleFor(x => x.Password)
            .ValidPassword(isRequired: true);
    }
}
