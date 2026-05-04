using FluentValidation;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortenerAPI.Responses.ValidatorConfigs;

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
