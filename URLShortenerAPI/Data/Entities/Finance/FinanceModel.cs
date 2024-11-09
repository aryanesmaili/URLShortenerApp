﻿using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Data.Entities.User;

namespace URLShortenerAPI.Data.Entities.Finance
{
    internal class FinancialRecord
    {
        public int ID { get; set; }

        private double? _cachedBalance;
        public double Balance
        {
            get
            {
                if (!_cachedBalance.HasValue)
                {
                    _cachedBalance = Deposits.Where(x => x.IsSuccessful).Sum(x => x.Amount) - Purchases.Sum(x => x.Amount);
                }
                return _cachedBalance.Value;
            }
        }

        public int UserID { get; set; }
        public required UserModel User { get; set; }

        public ICollection<DepositModel> Deposits { get; set; } = [];
        public ICollection<PurchaseModel> Purchases { get; set; } = [];
    }

    internal class DepositModel
    {
        public int ID { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSuccessful { get; set; }

        public int FinanceID { get; set; }
        public required FinancialRecord Finance { get; set; }
    }

    internal class PurchaseModel
    {
        public int ID { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Amount { get; set; }

        public int CustomURLID { get; set; }
        public required URLModel URL { get; set; }

        public int FinanceID { get; set; }
        public required FinancialRecord Finance { get; set; }
    }
}
