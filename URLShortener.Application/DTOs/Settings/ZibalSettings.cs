using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.Settings;

public sealed record ZibalSettings
{
    [Required]
    public required string RequestTransactionAddress { get; init; }

    [Required]
    public required string VerifyTransactionAddress { get; init; }

    [Required]
    public required string InquiryTransactionAddress { get; init; }

    [Required]
    public required string LazyRequestTransactionAddress { get; init; }

    [Required]
    public required string CallbackURL { get; init; }

    [Required]
    public required string RedirectURL { get; init; }

    [Required]
    public required string Merchant { get; init; }
}
