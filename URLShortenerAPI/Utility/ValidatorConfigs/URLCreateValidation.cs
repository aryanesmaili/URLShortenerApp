using FluentValidation;
using URLShortenerAPI.Data.Entities.URL;

namespace URLShortenerAPI.Utility.ValidatorConfigs
{
    public class URLCreateValidation : AbstractValidator<URLCreateDTO>
    {
        public URLCreateValidation()
        {
            RuleFor(x => x.LongURL)
                .NotEmpty().WithMessage("URL field cannot be empty.")
                .Matches("@\"\\b(?:https?://|www\\.)[^\\s/$.?#].[^\\s]*\\b\"").WithMessage("Given string is not a URL"); // Regex to determine URLs.
            RuleFor(x => x.IsActive).NotEmpty().WithMessage("IsActive field cannot be empty.");
            RuleFor(x => x.UserID).NotEmpty().WithMessage("The URL must belong to a user.");
        }
    }
}
