using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using URLShortener.Application.DTOs.EntityDTOs.Finance;
using URLShortener.Application.Interfaces.Services.Payment;
using URLShortener.Application.Repositories;
using URLShortener.Application.Utility.Exceptions;
using URLShortener.Common.Responses;
using URLShortener.Domain.Entities.Finance;
using URLShortener.Domain.Enums;
using URLShortener.Domain.Interfaces;
using URLShortener.Domain.Interfaces.Payment;
using URLShortener.Domain.ValueObjects.Payment;

namespace URLShortener.Infrastructure.Services.Payment;

public sealed class PaymentService : IPaymentService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapper _mapper;
    private readonly IDepositRepository _depositRepository;
    private readonly IFinancialRecordRepository _financialRecordRepository;

    public PaymentService(
                          IServiceProvider serviceProvider,
                          IMapper mapper,
                          IDepositRepository depositRepository,
                          IFinancialRecordRepository financialRecordRepository)
    {
        _serviceProvider = serviceProvider;
        _mapper = mapper;
        _depositRepository = depositRepository;
        _financialRecordRepository = financialRecordRepository;
    }

    /// <summary>
    /// Gets the list of a user's deposits.
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="username"></param>
    /// <returns>List of <see cref="DepositDTO"/></returns>
    public async Task<PagedResult<DepositDTO>> GetDepositsAsync(int pageNumber, int PageSize, long userID)
    {
        long financialRecordId = await _financialRecordRepository
            .Select(where => where.UserID == userID, fr => fr.ID) // Getting a single column
            .FirstOrDefaultAsync();

        var depositsPaged = await _depositRepository
            .GetPagedAsync(pageNumber, PageSize, x => x.FinanceID == financialRecordId, x => x.CreatedAt);
        return _mapper.Map<PagedResult<DepositDTO>>(depositsPaged);
    }

    public async Task<PaymentCreateResult> CreateTransactionAsync(PaymentTerminals paymentServices, PaymentCreateRequest requestInfo, long userId)
    {
        var service = _serviceProvider.GetKeyedService<IPaymentMethod>(paymentServices)
            ?? throw new ArgumentException($"Payment Method {paymentServices} Is Not Implemented.");

        // cast to the specific capability
        var transactionService = service as ICreateTransaction
            ?? throw new InvalidOperationException($"Creating Transaction Is Not Implemented for {paymentServices} with these models.");

        // Execute
        return await transactionService.CreateTransactionAsync(requestInfo, userId);
    }

    public async Task<PaymentVerifyResult> VerifyTransactionAsync(PaymentTerminals paymentServices, PaymentVerifyRequest requestInfo)
    {
        var service = _serviceProvider.GetKeyedService<IPaymentMethod>(paymentServices)
            ?? throw new ArgumentException($"Payment Method {paymentServices} Is Not Implemented.");

        var transactionService = service as IVerifyTransaction
            ?? throw new InvalidOperationException($"Verifying Transaction Is Not Implemented for {paymentServices} with these models.");

        return await transactionService.VerifyTransactionAsync(requestInfo);
    }

    public async Task<PaymentStatusResult> CheckTransactionStatusAsync(PaymentTerminals paymentServices, PaymentStatusRequest requestInfo, long userId)
    {
        // Check if this user has any such payments.
        var payment = await _financialRecordRepository.GetAsync(
            x => x.UserID == userId,
            i => i.Include(x => x.Deposits.Where(d => d.TrackID == requestInfo.TrackID)),
            asNoTracking: true
        );

        if (payment is null || payment.Deposits.Count == 0)
            throw new NotFoundException(nameof(DepositModel), nameof(DepositModel.TrackID), requestInfo.TrackID);

        var service = _serviceProvider.GetKeyedService<IPaymentMethod>(paymentServices)
            ?? throw new ArgumentException($"Payment Method {paymentServices} Is Not Implemented.");

        var transactionService = service as ICheckTransactionStatus
            ?? throw new InvalidOperationException($"Checking Transaction Status Is Not Implemented for {paymentServices} with these models.");

        return await transactionService.CheckTransactionStatusAsync(requestInfo);
    }
}
