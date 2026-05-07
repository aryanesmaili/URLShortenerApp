namespace URLShortener.Application.Features.Finance.DTOs;

public sealed record GetPaymentStatusRequest
{
    private string terminal = string.Empty;

    public string Terminal
    {
        get => terminal;
        init => terminal = value.Trim();
    }
    public long TrackID { get; init; }
}
