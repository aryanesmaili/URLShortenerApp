namespace URLShortener.Application.Features.Finance.DTOs;

public sealed record DepositDTO
{

    public int ID { get; init; }
    public double Amount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime PaidAt { get; init; }
    public bool IsSuccessful { get; init; }
    private string? failureReason;
    public string? FailureReason
    {
        get => failureReason;
        init => failureReason = value?.Trim();
    }
    public long? TrackID { get; init; }
    private string orderID = string.Empty;
    public string OrderID
    {
        get => orderID;
        init => orderID = value.Trim();
    }
    private string cardNumber = string.Empty;

    public string CardNumber
    {
        get => cardNumber;
        init => cardNumber = value.Trim();
    }
    public int? RefNumber { get; init; }
    private string? description;
    public string? Description
    {
        get => description;
        init => description = value?.Trim();
    }
    public int FinanceID { get; init; }
}
