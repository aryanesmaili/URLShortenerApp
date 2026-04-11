using URLShortener.Domain.Enums;

namespace URLShortener.Domain.Interfaces;

public interface IPaymentMethod
{
    public PaymentTerminals TerminalName { get; }
}
