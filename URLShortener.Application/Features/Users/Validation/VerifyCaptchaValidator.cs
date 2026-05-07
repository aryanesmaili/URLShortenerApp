using FluentValidation;
using URLShortener.Application.Features.Users.DTOs;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortener.Application.Features.Users.Validation;

public sealed class VerifyCaptchaValidator : AbstractValidator<VerifyCaptchaRequest>
{
    public VerifyCaptchaValidator()
    {
        RuleFor(x => x.Token)
            .IsRequired();
    }
}
