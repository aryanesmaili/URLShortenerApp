using FluentValidation;
using URLShortener.Application.Features.Users.DTOs;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortener.Application.Features.Users.Validation;

public sealed class CheckResetPasswordCodeValidator : AbstractValidator<CheckResetPasswordCodeRequest>
{
    public CheckResetPasswordCodeValidator()
    {
        RuleFor(x => x.Email)
            .IsRequired()
            .EmailAddress()
            .WithMessage("Email Is Not In Valid Format.");

        RuleFor(x => x.Token)
            .IsRequired();

        RuleFor(x => x.NewPassword)
            .ValidPassword(isRequired: true);
    }
}
