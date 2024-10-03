using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pexita.Utility.Exceptions;
using System.Security.Claims;
using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    internal class URLController : ControllerBase
    {
        private readonly IURLService _urlService;
        private readonly IValidator<URLCreateDTO> _validator;

        public URLController(IURLService urlService, IValidator<URLCreateDTO> validator)
        {
            _urlService = urlService;
            _validator = validator;
        }

        [HttpGet("/{id:int}")]
        public async Task<IActionResult> GetURL([FromBody] int id)
        {
            try
            {
                var result = await _urlService.GetURL(id);
                return Ok(result);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("AddURL")]
        public async Task<IActionResult> AddURL([FromBody] URLCreateDTO createDTO)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            try
            {
                await _validator.ValidateAndThrowAsync(createDTO);

                var result = _urlService.AddURL(createDTO, username!);
                return Ok(result);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
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
                return StatusCode(500, e.Message);
            }
        }
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
    }
}
