using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedDataModels.Responses;
using System.Security.Claims;
using URLShortener.Application.DTOs.EntityDTOs.Finance;
using URLShortener.Application.Interfaces.Services.Payment;
using URLShortener.Application.Utility.Exceptions;
using URLShortener.Common.Responses;
using URLShortener.Domain.Enums;
using URLShortener.Domain.ValueObjects.Payment;

namespace URLShortenerAPI.Controllers;

[ApiController]
[Route("api/[Controller]")]
public sealed class FinancialController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IMapper _mapper;

    public FinancialController(IPaymentService paymentService, IMapper mapper)
    {
        _paymentService = paymentService;
        _mapper = mapper;
    }

    [Authorize(Policy = "AllUsers")]
    [HttpGet("Balance")]
    [EnableRateLimiting("FetchData")]
    public async Task<IActionResult> GetUserBalance()
    {
        APIResponse<long> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var result = await _paymentService.GetUserBalance(userId);
            response = new()
            { Success = true, Result = result };
            return Ok(response);
        }

        catch (ArgumentException e)
        {
            response = new()
            { ErrorType = ErrorType.Argument, Message = e.Message };
            return BadRequest(response);
        }

        catch (NotFoundException e)
        {
            response = new()
            { ErrorType = ErrorType.NotFound, Message = e.Message };
            return NotFound(response);
        }

        catch (NotAuthorizedException e)
        {
            response = new()
            { ErrorType = ErrorType.Unauthorized, Message = e.Message };
            return BadRequest(response);
        }

        catch (Exception e)
        {
            DebugErrorResponse errorResponse = new()
            {
                Message = e.Message,
                InnerException = e.InnerException?.ToString() ?? "",
                StackTrace = e.StackTrace?.ToString() ?? ""
            };
            return StatusCode(500, errorResponse);
        }
    }

    #region Payment
    [Authorize(Policy = "AllUsers")]
    [HttpGet("Payments")]
    [EnableRateLimiting("FetchData")]
    public async Task<IActionResult> GetPaymentsOfUser([FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        APIResponse<PagedResult<DepositDTO>> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            PagedResult<DepositDTO> result = await _paymentService.GetDepositsAsync(pageNumber, pageSize, userId);
            response = new()
            { Success = true, Result = result };

            return Ok(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, Message = e.Message };
            return NotFound(response);
        }
        catch (ArgumentNullException e)
        {
            response = new() { ErrorType = ErrorType.ArgumentNullException, Message = e.Message };
            return BadRequest(response);
        }
        catch (NotAuthorizedException e)
        {
            response = new() { ErrorType = ErrorType.Unauthorized, Message = e.Message };
            return Unauthorized(response);
        }
        catch (Exception e)
        {
            DebugErrorResponse errorResponse = new()
            { Message = e.Message, InnerException = e.InnerException?.ToString() ?? "", StackTrace = e.StackTrace ?? "" };
            return StatusCode(500, errorResponse);
        }
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("CreateTransaction")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> CreateTransaction(PaymentCreateRequest createDTO)
    {
        APIResponse<string> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var result = await _paymentService.CreateTransactionAsync(createDTO.PaymentTerminal, createDTO, userId);

            if (result.Success)
                return Redirect(result.RedirectURL!);

            response = new() { Success = false, Result = result.Message };
            return BadRequest(response);
        }
        catch (Exception e)
        {
            DebugErrorResponse errorResponse = new()
            { Message = e.Message, InnerException = e.InnerException?.ToString() ?? "", StackTrace = e.StackTrace ?? "" };
            return StatusCode(500, errorResponse);
        }
    }

    [HttpGet("Zibal/Callback")]
    public async Task<IActionResult> GetCallback([FromQuery] int success, [FromQuery] long trackID, [FromQuery] string orderID, [FromQuery] int status)
    {
        APIResponse<PaymentVerifyResult> response;
        bool successfulOperation = success == 1;
        try
        {
            if (!successfulOperation)
            {
                response = new()
                {
                    Success = successfulOperation,
                    Result = new() { Amount = default, Success = false, Message = status.ToString(), RefNumber = default }
                };
                return BadRequest(response);
            }
            var result = await _paymentService.VerifyTransactionAsync(PaymentTerminals.Zibal, new() { TrackID = trackID });
            response = new()
            { Success = true, Result = result };
            return Ok(response);
        }
        catch (Exception e)
        {
            DebugErrorResponse errorResponse = new()
            { Message = e.Message, InnerException = e.InnerException?.ToString() ?? "", StackTrace = e.StackTrace ?? "" };
            return StatusCode(500, errorResponse);
        }
    }

    [Authorize(Policy = "AllUsers")]
    [HttpGet("CheckStatus")]
    [EnableRateLimiting("FetchData")]
    public async Task<IActionResult> GetPaymentStatus([FromQuery] string terminal, [FromQuery] long trackId)
    {
        APIResponse<PaymentStatusResult> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            PaymentTerminals paymentTerminal = Enum.Parse<PaymentTerminals>(terminal);
            PaymentStatusRequest request = new() { TrackID = trackId };
            var result = await _paymentService.CheckTransactionStatusAsync(paymentTerminal, request, userId);
            response = new()
            { Success = true, Result = result };
            return Ok(response);
        }
        catch (Exception e)
        {
            DebugErrorResponse errorResponse = new()
            { Message = e.Message, InnerException = e.InnerException?.ToString() ?? "", StackTrace = e.StackTrace ?? "" };
            return StatusCode(500, errorResponse);
        }
    }
    #endregion
}
