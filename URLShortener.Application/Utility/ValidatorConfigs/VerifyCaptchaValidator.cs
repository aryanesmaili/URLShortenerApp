using FluentValidation;
using URLShortener.Application.DTOs.EntityDTOs.User;

namespace URLShortener.Application.Utility.ValidatorConfigs;

public sealed class VerifyCaptchaValidator : AbstractValidator<VerifyCaptchaRequest>
{
    public VerifyCaptchaValidator()
    {
        RuleFor(x => x.Token)
            .IsRequired();
    }
}
