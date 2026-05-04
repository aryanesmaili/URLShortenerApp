using FluentValidation;
using URLShortener.Application.DTOs.EntityDTOs.Finance;

namespace URLShortener.Application.Utility.ValidatorConfigs;

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
