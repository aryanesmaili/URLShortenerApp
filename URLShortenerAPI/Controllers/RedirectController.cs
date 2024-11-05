using Microsoft.AspNetCore.Mvc;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using URLShortenerAPI.Data.Entities.Analytics;
using URLShortenerAPI.Services.Interfaces;
using URLShortenerAPI.Utility.Exceptions;

namespace URLShortenerAPI.Controllers
{
    [ApiController]
    [Route("/")]
    public class RedirectController : ControllerBase
    {
        private readonly IRedirectService _redirectService;

        public RedirectController(IRedirectService redirectService)
        {
            _redirectService = redirectService;
        }

        [HttpGet("{shortCode}")]
        public async Task<IActionResult> QuickLookup([FromRoute] string shortCode)
        {
            try
            {
                return await _redirectService.QuickLookup(shortCode) ? Redirect($"http://localhost:5083/RedirectURL/{shortCode}") : Redirect("http://localhost:5083/Notfound");
            }
            catch (Exception e)
            {
                DebugErrorResponse error =
                    new()
                    {
                        Message = e.Message,
                        InnerException = e.InnerException?.ToString() ?? "",
                        StackTrace = e.StackTrace ?? ""
                    };
                return StatusCode(500, error);
            }
        }

        [HttpGet("Resolve/{shortcode}")]
        public async Task<IActionResult> ResolveURL(string shortcode)
        {
            APIResponse<URLDTO> response;
            try
            {
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                string userAgent = HttpContext.Request.Headers.UserAgent.ToString();

                URLDTO result = await _redirectService.ResolveURL(shortcode, new IncomingRequestInfo { IPAddress = ipAddress!, UserAgent = userAgent, TimeClicked = DateTime.UtcNow });

                response = new()
                { Success = true, Result = result };
                return Ok(response);
            }
            catch (NotFoundException e)
            {
                response = new()
                { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message, };

                return NotFound(response);
            }
            catch (ArgumentException e)
            {
                response = new()
                { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };

                return BadRequest(response);
            }
            catch (Exception e)
            {
                DebugErrorResponse error =
                    new()
                    {
                        Message = e.Message,
                        InnerException = e.InnerException?.ToString() ?? "",
                        StackTrace = e.StackTrace ?? ""
                    };
                return StatusCode(500, error);
            }
        }
    }
}
