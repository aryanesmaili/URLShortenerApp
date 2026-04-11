using URLShortener.Domain.Enums;

namespace URLShortener.Domain.ValueObjects.Payment;

public sealed record PaymentCreateRequest
{
    /// <summary>
    /// Amount to pay (in Rials)
    /// </summary>
    public required long Amount { get; set; }

    private string? _description;
    /// <summary>
    /// Description regarding the Purchase. (Optional)
    /// </summary>
    public string? Description { get => _description; init => _description = value?.Trim(); }

    private string? mobile;
    /// <summary>
    /// If This Value if set, the user will see their registered card numbers in Zibal. (Optional)
    /// </summary>
    public string? Mobile { get => mobile; init => mobile = value?.Trim(); }

    /// <summary>
    /// The payment provider the user wants to pay at.
    /// Examples: Zibal, Zarinpal etc.
    /// </summary>
    public PaymentTerminals PaymentTerminal { get; init; }

}
