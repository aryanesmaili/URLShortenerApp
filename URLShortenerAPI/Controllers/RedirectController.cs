using Microsoft.AspNetCore.Mvc;
using Pexita.Utility.Exceptions;
using URLShortenerAPI.Data;
using URLShortenerAPI.Services.Interfaces;
using URLShortenerAPI.Data.Entities.Analytics;
namespace URLShortenerAPI.Controllers
{
    [ApiController]
    [Route("[Controller]")]
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
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                var headers = HttpContext.Request.Headers.ToDictionary(a => a.Key,a=>a.Value.ToArray().FirstOrDefault());

                var result = await _redirectService.ResolveURL(shortCode, new IncomingRequestInfo { IPAddress = ipAddress!, UserAgent = userAgent, Headers = headers });
                return Ok(result);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
