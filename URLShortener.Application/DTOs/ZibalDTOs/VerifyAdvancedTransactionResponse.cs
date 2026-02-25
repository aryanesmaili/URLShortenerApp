namespace URLShortener.Application.DTOs.ZibalDTOs
{
    /// <summary>
    /// The Placeholder for the response Zibal Sends to you.
    /// </summary>
    public class VerifyAdvancedTransactionResponse : VerifyTransactionResponse
    {
        /// <summary>
        /// information regarding the beneficiaries each in one item.
        /// </summary>
        public required List<MultiplexingInformation> MultiplexingInfos { get; set; }
    }

}
