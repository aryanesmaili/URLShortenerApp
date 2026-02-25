using URLShortener.Application.DTOs.ZibalDTOs;

namespace URLShortener.Application.Interfaces.Infrastructure.External;

public interface IZibalService
{
    Task<InquiryTransactionResponse> GetTransactionStatusAsync(InquiryTransactionRequest inquiryTransactionRequest, bool isAdvanced = false);
    Task<CreateTransactionResponse> RequestTransactionAsync(CreateTransactionRequest transactionInfo, bool isLazy = false, bool isAdvanced = false);
    Task<VerifyTransactionResponse> VerifyTransactionAsync(VerifyTransactionRequest verifyTransactionRequest, bool isAdvanced = false);
}
