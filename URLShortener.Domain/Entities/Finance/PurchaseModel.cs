using URLShortener.Domain.Entities.URL;

namespace URLShortener.Domain.Entities.Finance;

public class PurchaseModel
{
    public int ID { get; set; }
    public DateTime CreatedAt { get; set; }
    public double Amount { get; set; }

    public int CustomURLID { get; set; }
    public required URLModel URL { get; set; }

    public int FinanceID { get; set; }
    public required FinancialRecordModel Finance { get; set; }
}
