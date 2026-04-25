using URLShortener.Domain.Enums;

namespace URLShortener.Domain.Interfaces;

public interface IPaymentMethod
{
    PaymentTerminals TerminalName { get; }
}
