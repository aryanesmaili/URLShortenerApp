using FluentValidation;
using URLShortener.Domain.ValueObjects.Payment;

namespace URLShortener.Application.Utility.ValidatorConfigs;

public sealed class PaymentCreateValidator : AbstractValidator<PaymentCreateRequest>
{
    public PaymentCreateValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount must be greater than or equal to 0.");

        RuleFor(x => x.Mobile)
            .ValidMobileNumber(isRequired: false);

        RuleFor(x => x.PaymentTerminal)
            .IsRequired();

    }
}
