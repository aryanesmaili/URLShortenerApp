namespace URLShortener.Application.DTOs.Settings;

public sealed record PaymentSettings
{
    public required string CallbackURL { get; init; }
    public required string MerchantName { get; init; }
}
