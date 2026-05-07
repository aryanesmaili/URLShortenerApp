using FluentValidation;
using URLShortener.Application.Features.Users.DTOs;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortener.Application.Features.Users.Validation;

public sealed class CheckResetEmailCodeValidator : AbstractValidator<CheckResetEmailCodeRequest>
{
    public CheckResetEmailCodeValidator()
    {
        RuleFor(x => x.NewEmail)
            .IsRequired()
            .EmailAddress()
            .WithMessage("New Email Is Not In A Valid Format.");
    }
}
