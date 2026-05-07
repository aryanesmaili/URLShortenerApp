using FluentValidation;
using URLShortener.Application.Features.Users.DTOs;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortener.Application.Features.Users.Validation;

public class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequest>
{
    public ChangeEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .IsRequired()
            .EmailAddress()
            .WithMessage("New Email Is Not In Valid Format.");
    }
}
