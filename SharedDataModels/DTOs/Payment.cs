namespace SharedDataModels.DTOs
{
    public class PaymentCreateDTO
    {
        /// <summary>
        /// Amount to pay (in Rials)
        /// </summary>
        public required long Amount { get; set; }

        private string? _description;
        /// <summary>
        /// Description regarding the Purchase. (Optional)
        /// </summary>
        public string? Description { get => _description; set => _description = value?.Trim(); }

        private string? mobile;
        /// <summary>
        /// If This Value if set, the user will see their registered card numbers in Zibal. (Optional)
        /// </summary>
        public string? Mobile { get => mobile; set => mobile = value?.Trim(); }

        /// <summary>
        /// ID Of the User trying to perform this transaction.
        /// </summary>
        public int UserID { get; set; }
    }

    public class DepositDTO
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
}
