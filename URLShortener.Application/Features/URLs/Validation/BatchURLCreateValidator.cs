using FluentValidation;
using URLShortener.Application.Features.URLs.DTOs;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortener.Application.Features.URLs.Validation;

public sealed class BatchURLCreateValidator : AbstractValidator<BatchURLCreateDTO>
{
    public BatchURLCreateValidator()
    {
        RuleForEach(x => x.URLs)
            .ChildRules(url => url.RuleFor(u => u.LongURL).ValidURL(isRequired: true));
    }
}
