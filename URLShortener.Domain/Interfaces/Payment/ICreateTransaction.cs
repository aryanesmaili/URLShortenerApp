using URLShortener.Domain.ValueObjects.Payment;

namespace URLShortener.Domain.Interfaces.Payment;

public interface ICreateTransaction
{
    public Task<PaymentCreateResult> CreateTransactionAsync(PaymentCreateRequest request, long userId);
}
