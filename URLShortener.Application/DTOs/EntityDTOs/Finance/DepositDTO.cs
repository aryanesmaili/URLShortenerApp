namespace URLShortener.Application.DTOs.EntityDTOs.Finance;

public sealed class DepositDTO
{
    public int ID { get; set; }
    public double Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime PaidAt { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public long? TrackID { get; set; }
    public string OrderID { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public int? RefNumber { get; set; }
    public string? Description { get; set; }


    public int FinanceID { get; set; }
}
