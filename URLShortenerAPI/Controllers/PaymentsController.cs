using IPinfo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using URLShortenerAPI.Data.Interfaces.User;
using URLShortenerAPI.Services.User;
using URLShortenerAPI.Utility.Exceptions;

namespace URLShortenerAPI.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    internal class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize(Policy = "AllUsers")]
        [HttpGet("Payments/{id:int}")]
        public async Task<IActionResult> GetPaymentsOfUser(int userID)
        {
            APIResponse<List<DepositDTO>> response;
            string? username = HttpContext.User.Identity?.Name;
            try
            {
                List<DepositDTO> result = await _paymentService.GetDepositsAsync(userID, username!);
                response = new()
                { Success = true, Result = result };

                return Ok(response);
            }
            catch (NotFoundException e)
            {
                response = new() { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
                return NotFound(response);
            }
            catch (ArgumentNullException e)
            {
                response = new() { ErrorType = ErrorType.ArgumentNullException, ErrorMessage = e.Message };
                return BadRequest(response);
            }
            catch (NotAuthorizedException e)
            {
                response = new() { ErrorType = ErrorType.NotAuthorizedException, ErrorMessage = e.Message };
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
        public async Task<IActionResult> CreateTransaction(PaymentCreateDTO createDTO)
        {
            APIResponse<string> response;
            string? username = HttpContext.User.Identity?.Name;
            try
            {
                CreateTransactionResponse result = await _paymentService.CreateTransactionAsync(createDTO, username!);
                if (result.Result == 100)
                {
                    return Redirect($"https://gateway.zibal.ir/start/{result.TrackID}");
                }
                response = new() { Success = false, Result = $"ErrorCode:{result.Result} : {result.Message}" };
                return BadRequest(response);
            }
            catch (Exception e)
            {
                DebugErrorResponse errorResponse = new()
                { Message = e.Message, InnerException = e.InnerException?.ToString() ?? "", StackTrace = e.StackTrace ?? "" };
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("callback")]
        public async Task<IActionResult> GetCallback([FromQuery] int success, [FromQuery] long trackID, [FromQuery] string orderID, [FromQuery] int status)
        {
            APIResponse<VerifyTransactionResponse> response;
            try
            {
                VerifyTransactionResponse result = await _paymentService.VerifyTransactionAsync(trackID);
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
        [HttpGet("CheckStatus/{trackID:int}")]
        public async Task<IActionResult> GetPaymentStatus(int trackID)
        {
            APIResponse<InquiryTransactionResponse> response;
            try
            {
                InquiryTransactionResponse result = await _paymentService.CheckTransactionStatusAsync(trackID);
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
    }
}
