using FluentValidation;
using URLShortener.Application.DTOs.EntityDTOs.URL;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortenerAPI.Responses.ValidatorConfigs;

public sealed class BatchURLCreateValidator : AbstractValidator<BatchURLCreateDTO>
{
    public BatchURLCreateValidator()
    {
        RuleForEach(x => x.URLs)
            .ChildRules(url => url.RuleFor(u => u.LongURL).ValidURL(isRequired: true));
    }
}
