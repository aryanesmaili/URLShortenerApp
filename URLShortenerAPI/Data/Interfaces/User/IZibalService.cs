using URLShortenerAPI.Services.User;

namespace URLShortenerAPI.Data.Interfaces.User
{
    internal interface IZibalService
    {
        Task<InquiryTransactionResponse> GetTransactionStatusAsync(InquiryTransactionRequest inquiryTransactionRequest, bool isAdvanced = false);
        Task<CreateTransactionResponse> RequestTransactionAsync(CreateTransactionRequest transactionInfo, bool isLazy = false, bool isAdvanced = false);
        Task<VerifyTransactionResponse> VerifyTransactionAsync(VerifyTransactionRequest verifyTransactionRequest, bool isAdvanced = false);
    }
}