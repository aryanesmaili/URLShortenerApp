using Microsoft.AspNetCore.Mvc;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using URLShortenerAPI.Data.Entities.Analytics;
using URLShortenerAPI.Data.Interfaces.URL;
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
        public async Task<IActionResult> CheckURLExists([FromRoute] string shortCode)
        {
            try
            {
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                string userAgent = HttpContext.Request.Headers.UserAgent.ToString();

                URLDTO result = await _redirectService.CheckURLExists(shortCode, new IncomingRequestInfo { IPAddress = ipAddress!, UserAgent = userAgent, TimeClicked = DateTime.UtcNow });
                return result.IsMonetized ? Redirect(result.LongURL) : Redirect($"http://localhost:7112/RedirectURL/{shortCode}");
            }

            catch (NotFoundException)
            {
                return Redirect("http://localhost:7112/Notfound");
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
                URLDTO result = await _redirectService.ResolveURL(shortcode);

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
