﻿using FluentValidation;
using SharedDataModels.DTOs;

namespace URLShortenerAPI.Utility.ValidatorConfigs
{
    public class URLCreateValidation : AbstractValidator<URLCreateDTO>
    {
        public URLCreateValidation()
        {
            RuleFor(x => x.LongURL)
                .NotEmpty().WithMessage("URL field cannot be empty.")
                .Matches(@"^(https?:\/\/)?(www\.)?[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})+([\/\w-]*)*\/?$").WithMessage("Given string is not a URL"); // Regex to determine URLs.
            RuleFor(x => x.IsActive).NotNull().WithMessage("IsActive field cannot be empty.");
            RuleFor(x => x.UserID).NotEmpty().WithMessage("The URL must belong to a user.");
        }
    }
    public class ListURLCreateValidation : AbstractValidator<List<URLCreateDTO>>
    {
        public ListURLCreateValidation()
        {
            RuleForEach(x => x).SetValidator(new URLCreateValidation());
        }
    }
}
