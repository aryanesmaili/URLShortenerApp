namespace URLShortener.Application.DTOs.ZibalDTOs
{
    /// <summary>
    /// The Placeholder to provide information for each of the multiplexing items.
    /// </summary>
    public class MultiplexingInformation
    {
        private string? bankAccount;
        /// <summary>
        /// SHABA Number of the beneficiary.
        /// Specify Only One of these : BankAccount, SubMerchantID, WalletID
        /// </summary>
        public string? BankAccount { get => bankAccount; set => bankAccount = value?.Trim(); }

        private string? subMerchantID;
        /// <summary>
        /// ID of the beneficiary.
        /// Specify Only One of these : BankAccount, SubMerchantID, WalletID
        /// </summary>
        public string? SubMerchantID { get => subMerchantID; set => subMerchantID = value?.Trim(); }

        private string? walletID;
        /// <summary>
        /// Wallet ID. Not supported in "پرداختیاری".
        /// Specify Only One of these : BankAccount, SubMerchantID, WalletID
        /// </summary>
        public string? WalletID { get => walletID; set => walletID = value?.Trim(); }

        /// <summary>
        /// Amount or Percent.
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// Should the fee be paid by this item? effective only  when "FeeMode" is set to 0
        /// if not specified, the main beneficiary will be charged.
        /// </summary>
        public bool WagePayer { get; set; }
    }

}
