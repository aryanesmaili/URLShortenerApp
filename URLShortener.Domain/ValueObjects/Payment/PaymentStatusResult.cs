namespace URLShortener.Domain.ValueObjects.Payment;

public sealed record PaymentStatusResult
{
    /// <summary>
    /// Date and time when the payment was completed.
    /// </summary>
    public DateTime PaidAt { get; init; }

    /// <summary>
    /// Card number (masked) used for the payment, if provided by the provider.
    /// </summary>
    public string? CardNumber { get; init; }

    /// <summary>
    /// Indicates whether the payment was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Paid amount (in Rials).
    /// </summary>
    public long Amount { get; init; }

    /// <summary>
    /// Reference number issued by the provider.
    /// </summary>
    public long RefNumber { get; init; }

    /// <summary>
    /// Optional payment description or provider note.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The order identifier associated with this payment.
    /// </summary>
    public required string OrderID { get; init; }
}
