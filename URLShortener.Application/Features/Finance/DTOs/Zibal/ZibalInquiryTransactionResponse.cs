namespace URLShortener.Application.Features.Finance.DTOs.Zibal;

/// <summary>
/// The Placeholder for inquiry response, it's almost the same as <see cref="ZibalVerifyTransactionResponse"/> but with a few extra fields.
/// </summary>
public sealed record ZibalInquiryTransactionResponse : ZibalVerifyTransactionResponse
{
    /// <summary>
    /// The <see cref="DateTime"/> When The Transaction was Made.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The <see cref="DateTime"/> When The Transaction was Verified.
    /// </summary>
    public DateTime VerifiedAt { get; set; }

    /// <summary>
    /// 0 Means paid from transaction, 1 Means paid from Fee Wallet, 2 Means paid by the customer.
    /// </summary>
    public int Wage { get; set; }
}
