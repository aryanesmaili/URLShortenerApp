using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using URLShortener.Application.Common.Exceptions;
using URLShortener.Application.Common.Models;
using URLShortener.Application.Configuration;
using URLShortener.Application.Features.URLs.DTOs;
using URLShortener.Application.Features.URLs.Interfaces.Services;
using URLShortenerAPI.Utility;

namespace URLShortenerAPI.Controllers;

[ApiController]
[Route("/")]
public sealed class RedirectController(IRedirectService redirectService, IWebHostEnvironment webHostEnvironment, IOptions<ApplicationInfoSettings> appInfo) : ControllerBase
{
    private readonly IRedirectService _redirectService = redirectService;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    private readonly ApplicationInfoSettings _appInfo = appInfo.Value;

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> CheckURLExists([FromRoute] string shortCode)
    {
        string baseUrl = _appInfo.BaseUrl;

        try
        {
            IncomingRequestMetadata requestInfo = BuildRequestInfo();

            URLDTO result = await _redirectService.CheckURLExists(shortCode, requestInfo);

            if (result.IsMonetized)
                return Redirect($"{baseUrl}/RedirectURL/{shortCode}");

            return Redirect(result.LongURL);
        }
        catch (NotFoundException)
        {
            return Redirect($"{baseUrl}/Notfound");
        }
    }

    private IncomingRequestMetadata BuildRequestInfo()
    {
        return new IncomingRequestMetadata
        {
            IPAddress = GetClientIpAddress(),
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
            TimeClicked = DateTime.UtcNow
        };
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
            ?? HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString()
            ?? "Unknown";
    }

    [HttpGet("Resolve/{shortcode}")]
    public async Task<IActionResult> ResolveURL(string shortcode)
    {
        URLDTO result = await _redirectService.ResolveShortCode(shortcode);
        var response = CreateResult.Success(result);
        return Ok(response);
    }
}
