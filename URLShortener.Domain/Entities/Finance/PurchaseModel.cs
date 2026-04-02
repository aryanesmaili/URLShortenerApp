using URLShortener.Domain.Entities.URL;

namespace URLShortener.Domain.Entities.Finance;

public class PurchaseModel
{
    public long ID { get; set; }
    public DateTime CreatedAt { get; set; }
    public long Amount { get; set; }

    public required int ServiceType { get; set; }

    public long CustomURLID { get; set; }
    public required URLModel URL { get; set; }

    public long FinanceID { get; set; }
    public required FinancialRecordModel Finance { get; set; }
}
