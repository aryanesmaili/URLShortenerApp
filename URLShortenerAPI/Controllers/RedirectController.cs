using Microsoft.AspNetCore.Mvc;
using URLShortener.Application.DTOs;
using URLShortener.Application.DTOs.EntityDTOs.URL;
using URLShortener.Application.Interfaces.Services.URL;
using URLShortener.Application.Utility.Exceptions;
using URLShortenerAPI.Utility;

namespace URLShortenerAPI.Controllers;

[ApiController]
[Route("/")]
public sealed class RedirectController : ControllerBase
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
        var baseUrl = GetBaseUrl();

        try
        {
            var requestInfo = BuildRequestInfo();

            var result = await _redirectService.CheckURLExists(shortCode, requestInfo);

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
        if (_webHostEnvironment.IsDevelopment())
        {
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
        }

        return HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
            ?? HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString()
            ?? "Unknown";
    }

    private string GetBaseUrl()
    {
        return _webHostEnvironment.IsDevelopment()
            ? "https://localhost:7112"
            : "http://Pexita.click";
    }

    [HttpGet("Resolve/{shortcode}")]
    public async Task<IActionResult> ResolveURL(string shortcode)
    {
        URLDTO result = await _redirectService.ResolveShortCode(shortcode);

        var response = CreateResult.CreateDataSuccess(result);
        return Ok(response);

    }
}
