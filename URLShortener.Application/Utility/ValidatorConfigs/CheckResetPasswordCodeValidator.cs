using FluentValidation;
using URLShortener.Application.DTOs.EntityDTOs.User;

namespace URLShortener.Application.Utility.ValidatorConfigs;

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
