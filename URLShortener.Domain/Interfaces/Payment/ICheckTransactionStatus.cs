using URLShortener.Domain.ValueObjects.Payment;

namespace URLShortener.Domain.Interfaces.Payment;

public interface ICheckTransactionStatus
{
    Task<PaymentStatusResult> CheckTransactionStatusAsync(PaymentStatusRequest request);
}
