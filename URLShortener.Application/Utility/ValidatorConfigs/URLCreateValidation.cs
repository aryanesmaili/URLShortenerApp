using FluentValidation;
using URLShortener.Application.DTOs.EntityDTOs.URL;

namespace URLShortenerAPI.Responses.ValidatorConfigs;

public sealed class URLCreateValidation : AbstractValidator<URLCreateDTO>
{
    public URLCreateValidation()
    {
        RuleFor(x => x.LongURL)
            .NotEmpty().WithMessage("URL field cannot be empty.")
            .Matches(@"^(https?:\/\/)?(www\.)?[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})+([\/\w-]*)*\/?$").WithMessage("Given string is not a URL"); // Regex to determine URLs.
        RuleFor(x => x.IsActive).NotNull().WithMessage("IsActive field cannot be empty.");
        //RuleFor(x => x.UserID).NotEmpty().WithMessage("The URL must belong to a user.");
        RuleFor(x => x.CustomShortCode)
            .MaximumLength(32)
            .When(x => !string.IsNullOrEmpty(x.CustomShortCode))
            .WithMessage("CustomShortCode can't exceed 32 characters.");
    }
}
public sealed class ListURLCreateValidation : AbstractValidator<BatchURLCreateDTO>
{
    public ListURLCreateValidation()
    {
        // TODO: FIX VALIDATORS
        // RuleForEach(x => x.URLs).SetValidator(new URLCreateValidation());
    }
}
