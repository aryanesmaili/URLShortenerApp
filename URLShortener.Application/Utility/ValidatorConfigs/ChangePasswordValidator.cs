using FluentValidation;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortenerAPI.Responses.ValidatorConfigs;

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.Identifier)
            .Cascade(CascadeMode.Stop)
            .EmailAddress()
            .WithMessage("Identifier must be a valid email address.")
            .ValidUsername(isRequired: true)
            .WithMessage("Identifier must be a valid username.");

    }
}
