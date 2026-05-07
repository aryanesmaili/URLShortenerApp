using FluentValidation;
using URLShortener.Application.Features.Finance.DTOs;
using URLShortener.Application.Utility.ValidatorConfigs;

namespace URLShortener.Application.Features.Finance.Validation;

public sealed class GetPaymentStatusValidator : AbstractValidator<GetPaymentStatusRequest>
{
    public GetPaymentStatusValidator()
    {
        RuleFor(x => x.Terminal)
            .IsRequired();

        RuleFor(x => x.TrackID)
            .IsRequired()
            .GreaterThan(0)
            .WithMessage("TrackID must be greater than 0.");
    }
}
