using URLShortener.Domain.Entities.User;

namespace URLShortener.Domain.Entities.Finance;

public class FinancialRecordModel
{
    public int ID { get; set; }

    public long Balance { get; set; }

    public int UserID { get; set; }
    public required UserModel User { get; set; }

    public ICollection<DepositModel> Deposits { get; set; } = [];
    public ICollection<PurchaseModel> Purchases { get; set; } = [];
}
