using URLShortener.Domain.ValueObjects.Payment;

namespace URLShortener.Domain.Interfaces.Payment;

public interface ICreateTransaction
{
    Task<PaymentCreateResult> CreateTransactionAsync(PaymentCreateRequest request, long userId);
}
