namespace URLShortener.Domain.ValueObjects.Payment;

public sealed record PaymentStatusRequest
{
    /// <summary>
    /// Provider-issued tracking ID for the payment.
    /// </summary>
    public required long TrackID { get; init; }
}