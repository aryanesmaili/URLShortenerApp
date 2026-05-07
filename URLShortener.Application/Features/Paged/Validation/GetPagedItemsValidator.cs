using FluentValidation;
using URLShortener.Application.Features.Paged.DTOs;

namespace URLShortener.Application.Features.Paged.Validation;

public sealed class GetPagedItemsValidator : AbstractValidator<GetPagedItemsRequest>
{
    public GetPagedItemsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithErrorCode("INVALID_PAGE_NUMBER")
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithErrorCode("INVALID_PAGE_SIZE")
            .WithMessage("Page size must be between 1 and 100.");
    }
}
