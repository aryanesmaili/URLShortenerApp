using AutoMapper;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.DTOs.ZibalDTOs;
using URLShortener.Domain.Enums;
using URLShortener.Domain.Interfaces;
using URLShortener.Domain.Interfaces.Payment;
using URLShortener.Domain.ValueObjects.Payment;

namespace URLShortener.Infrastructure.Services.Payment.PaymentProviders;

public sealed class ZibalPayment : IPaymentMethod,
    ICreateTransaction,
    IVerifyTransaction,
    ICheckTransactionStatus
{
    private readonly HttpClient _httpClient;
    private readonly ZibalSettings _settings;
    private readonly IMapper _mapper;

    public ZibalPayment(HttpClient httpClient, IOptions<ZibalSettings> settings, IMapper mapper)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _mapper = mapper;
    }

    public PaymentTerminals TerminalName => PaymentTerminals.Zibal;

    public async Task<PaymentCreateResult> CreateTransactionAsync(PaymentCreateRequest request, long userId)
    {
        ZibalCreateTransactionRequest requestInfo = PrepareCreateTransactionPayload(request, userId);
        ZibalCreateTransactionResponse apiResult = await PostAsync<ZibalCreateTransactionRequest, ZibalCreateTransactionResponse>(
            _settings.RequestTransactionAddress,
            requestInfo
        );
        return _mapper.Map<PaymentCreateResult>(apiResult);
    }

    private ZibalCreateTransactionRequest PrepareCreateTransactionPayload(PaymentCreateRequest request, long userId)
    {
        var info = _mapper.Map<ZibalCreateTransactionRequest>(request);
        info.CallbackURL = _settings.CallbackURL;
        info.Merchant = _settings.Merchant;
        info.OrderID = GenerateRandomOrderID(userId);
        return info;
    }

    /// <summary>
    /// Generates a OrderID for the deposit to happen.
    /// </summary>
    /// <param name="userID"></param>
    /// <returns></returns>
    private static string GenerateRandomOrderID(long userID) =>
         $"{userID}_{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

    public async Task<PaymentVerifyResult> VerifyTransactionAsync(PaymentVerifyRequest request)
    {
        // TODO: create mapping between these classes.
        var requestInfo = PrepareVerifyTransactionPayload(request);
        var apiResult = await PostAsync<ZibalVerifyTransactionRequest, ZibalVerifyTransactionResponse>(
            _settings.VerifyTransactionAddress,
            requestInfo
        );
        return _mapper.Map<PaymentVerifyResult>(apiResult);
    }

    private ZibalVerifyTransactionRequest PrepareVerifyTransactionPayload(PaymentVerifyRequest request)
    {
        var requestInfo = _mapper.Map<ZibalVerifyTransactionRequest>(request);
        requestInfo.Merchant = _settings.Merchant;
        return requestInfo;
    }

    public async Task<PaymentStatusResult> CheckTransactionStatusAsync(PaymentStatusRequest request)
    {
        var requestInfo = PrepareTransactionStatusPayload(request);
        var apiResult = await PostAsync<ZibalInquiryTransactionRequest, ZibalInquiryTransactionResponse>(
            _settings.InquiryTransactionAddress,
            requestInfo
        );
        return _mapper.Map<PaymentStatusResult>(apiResult);
    }

    private ZibalInquiryTransactionRequest PrepareTransactionStatusPayload(PaymentStatusRequest request)
    {
        var requestInfo = _mapper.Map<ZibalInquiryTransactionRequest>(request);
        requestInfo.Merchant = _settings.Merchant;
        return requestInfo;
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest payload)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, payload);

        response.EnsureSuccessStatusCode();

        TResponse? result =
            await response.Content.ReadFromJsonAsync<TResponse>();

        return result!;
    }
}
