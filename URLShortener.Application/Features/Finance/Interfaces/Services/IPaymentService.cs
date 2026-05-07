using URLShortener.Application.Features.Finance.DTOs;
using URLShortener.Common.Responses;
using URLShortener.Domain.Enums;
using URLShortener.Domain.ValueObjects.Payment;

namespace URLShortener.Application.Features.Finance.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentCreateResult> CreateTransactionAsync(PaymentTerminals paymentServices, PaymentCreateRequest requestInfo, long userId);

    Task<PaymentVerifyResult> VerifyTransactionAsync(PaymentTerminals paymentServices, PaymentVerifyRequest requestInfo);

    Task<PaymentStatusResult> CheckTransactionStatusAsync(PaymentTerminals paymentServices, PaymentStatusRequest requestInfo, long userId);

    Task<PagedResult<DepositDTO>> GetDepositsAsync(int pageNumber, int PageSize, long userID);
    Task<long> GetUserBalance(long userId);
}
