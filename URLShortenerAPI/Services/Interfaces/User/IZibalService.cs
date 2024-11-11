using URLShortenerAPI.Services.User;

namespace URLShortenerAPI.Services.Interfaces.UserRelated
{
    internal interface IZibalService
    {
        Task<InquiryTransactionResponse> GetTransactionStatus(InquiryTransactionRequest inquiryTransactionRequest, bool isAdvanced = false);
        Task<CreateTransactionResponse> RequestTransaction(CreateTransactionRequest transactionInfo, bool isLazy = false, bool isAdvanced = false);
        Task<VerifyTransactionResponse> VerifyTransaction(VerifyTransactionRequest verifyTransactionRequest, bool isAdvanced = false);
    }
}