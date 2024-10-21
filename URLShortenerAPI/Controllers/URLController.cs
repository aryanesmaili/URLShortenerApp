using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedDataModels.DTOs;
using System.Security.Claims;
using URLShortenerAPI.Services.Interfaces;
using SharedDataModels.Utility.Exceptions;
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
            try
            {
                URLDTO result = await _urlService.GetURL(id);
                return Ok(result);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.StackTrace);
            }
        }

        [Authorize(Policy = "AllUsers")]
        [HttpPost("AddURL")]
        public async Task<IActionResult> AddURL([FromBody] URLCreateDTO createDTO)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            try
            {
                await _validator.ValidateAndThrowAsync(createDTO);

                URLDTO result = await _urlService.AddURL(createDTO, username!);
                return Ok(result);
            }

            catch (ValidationException e)
            {
                List<string> errors = [];

                foreach (var error in e.Errors)
                {
                    errors.Add($"{error.PropertyName}: {error.ErrorMessage}");
                }

                return BadRequest(errors);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (NotAuthorizedException e)
            {
                return Unauthorized(e.Message);
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
            var username = User.FindFirstValue(ClaimTypes.Name);
            try
            {
                await _listValidator.ValidateAndThrowAsync(createDTO);

                var result = await _urlService.AddBatchURL(createDTO, username!);
                return Ok(result);
            }
            catch (ValidationException e)
            {
                List<string> errors = [];

                foreach (var error in e.Errors)
                {
                    errors.Add($"{error.PropertyName}: {error.ErrorMessage}");
                }

                return BadRequest(errors);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ArgumentNullException e)
            {
                return BadRequest(e.Message);
            }
            catch (NotAuthorizedException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                var error = new DebugErrorResponse
                { Message = e.Message, StackTrace = e.StackTrace };
                return StatusCode(500, error);
            }
        }
        [Authorize(Policy = "AllUsers")]
        [HttpPost("ToggleActivation/{id:int}")]
        public async Task<IActionResult> ToggleActivation(int id)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            try
            {
                await _urlService.ToggleActivation(id, username!);
                return Ok();
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (NotAuthorizedException e)
            {
                return Unauthorized(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
        [Authorize(Policy = "AllUsers")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> DeleteURL(int id)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            try
            {
                await _urlService.DeleteURL(id, username!);
                return Ok();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }

            catch (NotAuthorizedException e)
            {
                return Unauthorized(e.Message);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                DebugErrorResponse response = new()
                {
                    Message = e.Message,
                    InnerException = e.InnerException?.ToString() ?? "",
                    StackTrace = e.StackTrace?.ToString() ?? ""
                };
                return StatusCode(500, response);
            }
        }
    }
}
