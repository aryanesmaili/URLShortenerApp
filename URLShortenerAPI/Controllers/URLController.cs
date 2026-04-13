using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedDataModels.Responses;
using System.Security.Claims;
using URLShortener.Application.DTOs.EntityDTOs.URL;
using URLShortener.Application.Interfaces.Services.URL;
using URLShortener.Application.Utility.Exceptions;

namespace URLShortenerAPI.Controllers;

[ApiController]
[Route("api/[Controller]")]
public sealed class URLController : ControllerBase
{
    private readonly IURLService _urlService;
    private readonly IValidator<URLCreateDTO> _validator;
    private readonly IValidator<BatchURLCreateDTO> _batchURLValidator;
    public URLController(IURLService urlService, IValidator<URLCreateDTO> validator, IValidator<BatchURLCreateDTO> batchValidator)
    {
        _urlService = urlService;
        _validator = validator;
        _batchURLValidator = batchValidator;
    }

    [Authorize(Policy = "AllUsers")]
    [HttpGet("/{id:int}")]
    [EnableRateLimiting("FetchData")]
    public async Task<IActionResult> GetURL([FromRoute] int id)
    {
        APIResponse<URLDTO> response;
        try
        {
            URLDTO result = await _urlService.GetURLAsync(id);
            response = new()
            { Success = true, Result = result };
            return Ok(result);
        }
        catch (NotFoundException e)
        {
            response = new()
            { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }
        catch (Exception e)
        {
            DebugErrorResponse errorResponse = new()
            { Message = e.Message, InnerException = e.InnerException?.ToString() ?? "", StackTrace = e.StackTrace ?? "" };
            return StatusCode(500, errorResponse);
        }
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("AddURL")]
    [EnableRateLimiting("AddURL")]
    public async Task<IActionResult> AddURL([FromBody] URLCreateDTO createDTO)
    {
        APIResponse<URLShortenResponse> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _validator.ValidateAndThrowAsync(createDTO);

            URLShortenResponse result = await _urlService.AddURL(createDTO, userId!);
            response = new()
            { Success = true, Result = result };
            return Ok(response);
        }

        catch (InsufficientBalanceException e)
        {
            response = new()
            { Success = false, ErrorMessage = e.Message };
            return BadRequest(response);
        }

        catch (ValidationException e)
        {
            List<string> errors = [];

            foreach (var error in e.Errors)
            {
                errors.Add($"{error.PropertyName}: {error.ErrorMessage}");
            }
            response = new() { ErrorType = ErrorType.ValidationException, ErrorMessage = e.Message, Errors = errors };

            return BadRequest(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }
        catch (ArgumentException e)
        {
            response = new() { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
            return BadRequest(response);
        }
        catch (NotAuthorizedException e)
        {
            response = new() { ErrorType = ErrorType.NotAuthorizedException, ErrorMessage = e.Message };
            return Unauthorized(response);
        }
        catch (Exception e)
        {
            var error = new DebugErrorResponse
            { Message = e.Message, InnerException = e.InnerException?.ToString() ?? "", StackTrace = e.StackTrace ?? "" };
            return StatusCode(500, error);
        }
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("AddBatchURL")]
    [EnableRateLimiting("AddURL")]
    public async Task<IActionResult> AddBatchURL([FromBody] BatchURLCreateDTO createDTO)
    {
        APIResponse<IReadOnlyList<URLShortenResponse>> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _batchURLValidator.ValidateAndThrowAsync(createDTO);

            IReadOnlyList<URLShortenResponse> result = await _urlService.AddBatchURL(createDTO, userId!);
            response = new()
            { Success = true, Result = result };
            return Ok(response);
        }
        catch (ValidationException e)
        {
            List<string> errors = [];

            foreach (var error in e.Errors)
            {
                errors.Add($"{error.PropertyName}: {error.ErrorMessage}");
            }

            response = new() { ErrorType = ErrorType.ValidationException, ErrorMessage = e.Message, Errors = errors };
            return BadRequest(response);
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
            var error = new DebugErrorResponse
            { Message = e.Message, StackTrace = e.StackTrace ?? "", InnerException = e.InnerException?.ToString() ?? "" };
            return StatusCode(500, error);
        }
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("ToggleActivation/{id:int}")]
    [EnableRateLimiting("FetchData")]
    public async Task<IActionResult> ToggleActivation(int id)
    {
        APIResponse<string> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _urlService.ToggleStateAsync(id, x => x.IsActive = !x.IsActive, userId);
            response = new()
            { Success = true, Result = string.Empty };
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            response = new()
            { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
            return BadRequest(response);
        }
        catch (NotFoundException e)
        {
            response = new()
            { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }
        catch (NotAuthorizedException e)
        {
            response = new()
            { ErrorType = ErrorType.NotAuthorizedException, ErrorMessage = e.Message };
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
    [HttpPost("ToggleMonetization/{id:int}")]
    [EnableRateLimiting("FetchData")]
    public async Task<IActionResult> ToggleMonetization(int id)
    {
        APIResponse<string> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _urlService.ToggleStateAsync(id, x => x.IsMonetized = !x.IsMonetized, userId);
            response = new()
            { Success = true, Result = string.Empty };
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            response = new()
            { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
            return BadRequest(response);
        }
        catch (NotFoundException e)
        {
            response = new()
            { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }
        catch (NotAuthorizedException e)
        {
            response = new()
            { ErrorType = ErrorType.NotAuthorizedException, ErrorMessage = e.Message };
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
    [HttpDelete("Delete/{id:int}")]
    [EnableRateLimiting("Deletion")]
    public async Task<IActionResult> DeleteURL(int id)
    {
        APIResponse<string> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _urlService.DeleteURL(id, userId);
            response = new()
            { Success = true };
            return Ok(response);
        }

        catch (NotFoundException e)
        {
            response = new()
            { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }

        catch (NotAuthorizedException e)
        {
            response = new()
            { ErrorType = ErrorType.NotAuthorizedException, ErrorMessage = e.Message };
            return Unauthorized(response);
        }
        catch (ArgumentException e)
        {
            response = new()
            { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
            return BadRequest(e.Message);
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
}
