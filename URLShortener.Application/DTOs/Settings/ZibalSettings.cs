namespace URLShortener.Infrastructure.Services.Payment.PaymentProviders;

public sealed class ZibalSettings
{
    public required string RequestTransactionAddress { get; init; }
    public required string VerifyTransactionAddress { get; init; }
    public required string InquiryTransactionAddress { get; init; }
    public required string LazyRequestTransactionAddress { get; init; }
    public required string CallbackURL { get; init; }
    public required string RedirectURL { get; init; }
    public required string Merchant { get; init; }
}