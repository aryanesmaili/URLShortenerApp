namespace URLShortener.Domain.ValueObjects.Payment;

public sealed record PaymentCreateResult
{
    /// <summary>
    /// Indicates whether the payment initialization succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Optional provider message or error description.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// URL the user should be redirected to for completing the payment.
    /// </summary>
    public string? RedirectURL { get; set; }
}