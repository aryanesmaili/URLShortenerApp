using URLShortener.Domain.ValueObjects.Payment;

namespace URLShortener.Domain.Interfaces.Payment;

public interface IVerifyTransaction
{
    Task<PaymentVerifyResult> VerifyTransactionAsync(PaymentVerifyRequest request);
}
