namespace URLShortener.Application.DTOs.EntityDTOs.Finance;

public sealed record GetPaymentStatusRequest
{
    public string Terminal { get; init; } = string.Empty;
    public long TrackID { get; init; }
}
