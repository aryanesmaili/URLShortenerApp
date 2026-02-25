using URLShortener.Application.DTOs;
using URLShortener.Application.DTOs.ZibalDTOs;

namespace URLShortener.Application.Interfaces.Services.User
{
    public interface IPaymentService
    {
        Task<InquiryTransactionResponse> CheckTransactionStatusAsync(int trackID);
        Task<CreateTransactionResponse> CreateTransactionAsync(PaymentCreateDTO paymentCreate, string username);
        Task<List<DepositDTO>> GetDepositsAsync(int userID, string username);
        Task<VerifyTransactionResponse> VerifyTransactionAsync(long trackID);
    }
}