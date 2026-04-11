using URLShortener.Domain.ValueObjects.Payment;

namespace URLShortener.Domain.Interfaces.Payment;

public interface IVerifyTransaction
{
    public Task<PaymentVerifyResult> VerifyTransactionAsync(PaymentVerifyRequest request);
}
