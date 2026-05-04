using FluentValidation;
using URLShortener.Application.DTOs.EntityDTOs.User;

namespace URLShortener.Application.Utility.ValidatorConfigs;

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
