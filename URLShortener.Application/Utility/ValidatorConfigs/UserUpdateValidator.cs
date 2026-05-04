using FluentValidation;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortenerAPI.Responses.ValidatorConfigs;

public sealed class UserUpdateValidator : AbstractValidator<UserUpdateDTO>
{
    public UserUpdateValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Name) || !string.IsNullOrWhiteSpace(x.Username))
            .WithMessage("At least one of Name or Username must be filled.");

        RuleFor(x => x.Name)
            .ValidName(isRequired: false)
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Username)
            .ValidUsername(isRequired: false)
            .When(x => !string.IsNullOrWhiteSpace(x.Username));
    }
}
