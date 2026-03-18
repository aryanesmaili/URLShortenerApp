using URLShortener.Domain.Entities.User;

namespace URLShortener.Domain.Entities.Finance;

public class FinancialRecordModel
{
    public int ID { get; set; }

    private double _balance;
    public double Balance
    {
        get
        {
            return Math.Round(_balance, 2);
        }
        set
        {
            _balance = value;
        }
    }

    public int UserID { get; set; }
    public required UserModel User { get; set; }

    public ICollection<DepositModel> Deposits { get; set; } = [];
    public ICollection<PurchaseModel> Purchases { get; set; } = [];
}
