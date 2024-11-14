using SharedDataModels.DTOs;
using URLShortenerAPI.Services.User;

namespace URLShortenerAPI.Data.Interfaces.User
{
    internal interface IPaymentService
    {
        public Task<InquiryTransactionResponse> CheckTransactionStatusAsync(int trackID);
        public Task<CreateTransactionResponse> CreateTransactionAsync(PaymentCreateDTO paymentCreate, string username);
        public Task<List<DepositDTO>> GetDepositsAsync(int userID, string username);
        public Task<VerifyTransactionResponse> VerifyTransactionAsync(long trackID);
    }
}