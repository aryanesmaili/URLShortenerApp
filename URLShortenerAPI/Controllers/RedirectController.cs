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
        private readonly IWebHostEnvironment _webHostEnvironment;
        public RedirectController(IRedirectService redirectService, IWebHostEnvironment webHostEnvironment)
        {
            _redirectService = redirectService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("{shortCode}")]
        public async Task<IActionResult> CheckURLExists([FromRoute] string shortCode)
        {
            string url = _webHostEnvironment.IsDevelopment() ? "https://localhost:7112" : "http://Pexita.click";
            try
            {
                string? ipAddress;

                if (_webHostEnvironment.IsDevelopment())
                    ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();

                else // since the request is redirected by nginx, we have to retrieve IP from special headers.
                    ipAddress = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
                   ?? HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                   ?? HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString()
                   ?? "Unknown";

                string userAgent = HttpContext.Request.Headers.UserAgent.ToString();

                URLDTO result = await _redirectService.CheckURLExists(shortCode, new IncomingRequestInfo { IPAddress = ipAddress!, UserAgent = userAgent, TimeClicked = DateTime.UtcNow });


                return !result.IsMonetized ? Redirect(result.LongURL) : Redirect($"{url}/RedirectURL/{shortCode}");
            }

            catch (NotFoundException)
            {
                return Redirect($"{url}/Notfound");
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
