using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> RedirectToLongUrl([FromRoute] string shortCode)
        {
            APIResponse<string> response;
            try
            {
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                string userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                Dictionary<string, string?> headers = HttpContext.Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToArray().FirstOrDefault());

                string result = await _redirectService.ResolveURL(shortCode, new IncomingRequestInfo { IPAddress = ipAddress!, UserAgent = userAgent, Headers = headers, TimeClicked = DateTime.UtcNow });

                response = new()
                { Result = result };

                return Ok(response);
            }
            catch (NotFoundException e)
            {
                response = new()
                { ErrorType = ErrorType.NotFound, Message = e.Message, };

                return NotFound(response);
            }
            catch (ArgumentException e)
            {
                response = new()
                { ErrorType = ErrorType.ArgumentException, Message = e.Message };

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
