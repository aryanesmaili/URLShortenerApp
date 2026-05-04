using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedDataModels.Responses;
using System.Security.Claims;
using URLShortener.Application.DTOs.EntityDTOs;
using URLShortener.Application.DTOs.EntityDTOs.Finance;
using URLShortener.Application.Interfaces.Services.Payment;
using URLShortener.Common.Responses;
using URLShortener.Domain.Enums;
using URLShortener.Domain.ValueObjects.Payment;
using URLShortenerAPI.Utility;

namespace URLShortenerAPI.Controllers;

[ApiController]
[Route("api/[Controller]")]
public sealed class FinancialController(IPaymentService paymentService, IMapper mapper, IValidator<GetPagedItemsRequest> getPaymentsRequestValidator, IValidator<PaymentCreateRequest> paymentCreateRequestValidator, IValidator<GetPaymentStatusRequest> getPaymentStatusValidator) : ControllerBase
{
    private readonly IPaymentService _paymentService = paymentService;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<GetPagedItemsRequest> _getPaymentsRequestValidator = getPaymentsRequestValidator;
    private readonly IValidator<PaymentCreateRequest> _paymentCreateRequestValidator = paymentCreateRequestValidator;
    private readonly IValidator<GetPaymentStatusRequest> _getPaymentStatusValidator = getPaymentStatusValidator;

    [Authorize(Policy = "AllUsers")]
    [HttpGet("Balance")]
    [EnableRateLimiting("FetchData")]
    public async Task<IActionResult> GetUserBalance()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _paymentService.GetUserBalance(userId);
        var response = CreateResult.Success(result);
        return Ok(response);
    }

    #region Payment
    [Authorize(Policy = "AllUsers")]
    [HttpGet("Payments")]
    [EnableRateLimiting("FetchData")]
    public async Task<IActionResult> GetPaymentsOfUser([FromQuery] GetPagedItemsRequest reqInfo)
    {
        await _getPaymentsRequestValidator.ValidateAndThrowAsync(reqInfo);
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        PagedResult<DepositDTO> result = await _paymentService.GetDepositsAsync(reqInfo.PageNumber, reqInfo.PageSize, userId);
        var response = CreateResult.Paged(result);
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("CreateTransaction")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> CreateTransaction([FromBody] PaymentCreateRequest createDTO)
    {
        await _paymentCreateRequestValidator.ValidateAndThrowAsync(createDTO);
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _paymentService.CreateTransactionAsync(createDTO.PaymentTerminal, createDTO, userId);

        if (result.Success)
            return Redirect(result.RedirectURL!);
        else
            throw new Exception(result.Message);
    }

    [HttpGet("Zibal/Callback")]
    public async Task<IActionResult> GetCallback([FromQuery] int success, [FromQuery] long trackID, [FromQuery] string orderID, [FromQuery] int status)
    {
        bool successfulOperation = success == 1;
        if (!successfulOperation)
        {
            var failureResponse = CreateResult.Failure(ErrorType.PaymentFailed, status.ToString());
            return BadRequest(failureResponse);
        }
        var result = await _paymentService.VerifyTransactionAsync(PaymentTerminals.Zibal, new() { TrackID = trackID });
        var response = CreateResult.Success(result);
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpGet("CheckStatus")]
    [EnableRateLimiting("FetchData")]
    public async Task<IActionResult> GetPaymentStatus([FromQuery] GetPaymentStatusRequest reqInfo)
    {
        await _getPaymentStatusValidator.ValidateAndThrowAsync(reqInfo);
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (!Enum.TryParse<PaymentTerminals>(reqInfo.Terminal, out var paymentTerminal))
        {
            var failureResponse = CreateResult.Failure(ErrorType.Argument, "Invalid payment terminal.");
            return BadRequest(failureResponse);
        }
        PaymentStatusRequest request = new() { TrackID = reqInfo.TrackID };
        var result = await _paymentService.CheckTransactionStatusAsync(paymentTerminal, request, userId);
        var response = CreateResult.Success(result);
        return Ok(response);
    }
    #endregion
}
