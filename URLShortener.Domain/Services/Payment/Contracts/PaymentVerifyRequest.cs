namespace URLShortener.Domain.ValueObjects.Payment;

public sealed record PaymentVerifyRequest
{
    /// <summary>
    /// Provider-issued tracking ID used to verify a payment.
    /// </summary>
    public required long TrackID { get; init; }
}
