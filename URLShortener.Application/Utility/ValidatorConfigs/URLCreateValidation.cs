using FluentValidation;
using URLShortener.Application.DTOs.EntityDTOs.URL;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortenerAPI.Responses.ValidatorConfigs;

public sealed class URLCreateValidation : AbstractValidator<URLCreateDTO>
{
    public URLCreateValidation()
    {
        RuleFor(x => x.LongURL)
            .ValidURL(isRequired: true);

        // when provided, has to be between 3-32 characters, and should only contain alphanumeric characters, dashes, and underscores
        RuleFor(x => x.CustomShortCode)
            .Cascade(CascadeMode.Stop)
            .MinimumLength(3)
            .MaximumLength(32)
            .Matches("^[a-zA-Z0-9_-]+$")
            .When(x => !string.IsNullOrEmpty(x.CustomShortCode));
    }
}
