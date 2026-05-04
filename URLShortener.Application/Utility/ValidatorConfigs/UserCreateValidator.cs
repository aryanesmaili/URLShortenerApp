using FluentValidation;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortenerAPI.Responses.ValidatorConfigs;

public sealed class UserCreateValidator : AbstractValidator<UserCreateDTO>
{
    public UserCreateValidator()
    {
        RuleFor(x => x.Name)
            .ValidName(isRequired: true);

        RuleFor(x => x.Email)
            .ValidEmail(isRequired: true);

        RuleFor(x => x.Password)
            .ValidPassword(isRequired: true);

        RuleFor(x => x.ConfirmPassword)
            .ValidPassword(isRequired: true)
            .Equal(x => x.Password);

        RuleFor(x => x.Username)
            .ValidUsername(isRequired: true);
    }
}
