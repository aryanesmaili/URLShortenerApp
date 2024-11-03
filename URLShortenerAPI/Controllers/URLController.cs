using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using System.Security.Claims;
using URLShortenerAPI.Services.Interfaces;
using URLShortenerAPI.Utility.Exceptions;
namespace URLShortenerAPI.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class URLController : ControllerBase
    {
        private readonly IURLService _urlService;
        private readonly IValidator<URLCreateDTO> _validator;
        private readonly IValidator<List<URLCreateDTO>> _listValidator;
        public URLController(IURLService urlService, IValidator<URLCreateDTO> validator, IValidator<List<URLCreateDTO>> listValidator)
        {
            _urlService = urlService;
            _validator = validator;
            _listValidator = listValidator;
        }

        [Authorize(Policy = "AllUsers")]
        [HttpGet("/{id:int}")]
        public async Task<IActionResult> GetURL([FromRoute] int id)
        {
            APIResponse<URLDTO> response;
            try
            {
                URLDTO result = await _urlService.GetURL(id);
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
        public async Task<IActionResult> AddURL([FromBody] URLCreateDTO createDTO)
        {
            APIResponse<URLShortenResponse> response;
            var username = HttpContext.User.Identity?.Name;
            try
            {
                await _validator.ValidateAndThrowAsync(createDTO);

                URLShortenResponse result = await _urlService.AddURL(createDTO, username!);
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
        public async Task<IActionResult> AddBatchURL([FromBody] List<URLCreateDTO> createDTO)
        {
            APIResponse<List<URLShortenResponse>> response;
            var username = HttpContext.User.Identity?.Name;
            try
            {
                await _listValidator.ValidateAndThrowAsync(createDTO);

                List<URLShortenResponse> result = await _urlService.AddBatchURL(createDTO, username!);
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
        public async Task<IActionResult> ToggleActivation(int id)
        {
            APIResponse<string> response;
            var username = HttpContext.User.Identity?.Name;
            try
            {
                await _urlService.ToggleActivation(id, username!);
                response = new()
                { Success = true, };
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
        public async Task<IActionResult> DeleteURL(int id)
        {
            APIResponse<string> response;
            var username = HttpContext.User.Identity?.Name;
            try
            {
                await _urlService.DeleteURL(id, username!);
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
}
