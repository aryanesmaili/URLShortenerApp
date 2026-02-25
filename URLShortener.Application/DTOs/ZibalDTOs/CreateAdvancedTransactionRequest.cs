namespace URLShortener.Application.DTOs.ZibalDTOs
{
    /// <summary>
    /// The Placeholder for required data in order to make a Transaction in Advanced mode.
    /// </summary>
    public class CreateAdvancedTransactionRequest : CreateTransactionRequest
    {
        private int percentMode = 0;
        /// <summary>
        /// Specify if your multiplexing method is by percent. default to 0.
        /// </summary>
        public int PercentMode
        {
            get => percentMode;
            set
            {
                if (value == 1 || value == 0)

                    percentMode = value;
                else
                    throw new ArgumentException("Percent Mode must be 0 or 1");
            }
        }

        private int feeMode;
        /// <summary>
        /// The Method that the fee is paid in. 0 for paying from transaction, 1 for paying from the wallet connected, 2 for paid by the client. 
        /// </summary>
        public int FeeMode
        {
            get => feeMode;
            set
            {
                if (value == 0 || value == 1 || value == 2)
                    feeMode = value;
                else
                    throw new ArgumentException($"Fee Mode Value Must be 0 or 1 or 2");
            }
        }

        /// <summary>
        /// The List of beneficiaries in this transaction each as one Item.
        /// </summary>
        public required List<MultiplexingInformation> MultiplexingInfos { get; set; }
    }

}
