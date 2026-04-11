namespace URLShortener.Domain.ValueObjects.Payment;

public sealed record PaymentVerifyResult
{
    /// <summary>
    /// Indicates whether the verification was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Paid amount (in Rials).
    /// </summary>
    public required long Amount { get; init; }

    /// <summary>
    /// Provider-issued reference number (if available).
    /// </summary>
    public long? RefNumber { get; init; }

    /// <summary>
    /// Optional provider message or error description.
    /// </summary>
    public string? Message { get; init; }
}
