using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using URLShortener.Application.DTOs.EntityDTOs;
using URLShortener.Application.DTOs.EntityDTOs.URL;
using URLShortener.Application.Interfaces.Services.URL;
using URLShortener.Common.Responses;
using URLShortenerAPI.Utility;

namespace URLShortenerAPI.Controllers;

[ApiController]
[Route("api/[Controller]")]
public sealed class URLController(IURLService urlService, IValidator<URLCreateDTO> validator, IValidator<BatchURLCreateDTO> batchValidator, IValidator<GetPagedItemsRequest> getPagedItemsRequestValidator) : ControllerBase
{
    private readonly IURLService _urlService = urlService;
    private readonly IValidator<URLCreateDTO> _validator = validator;
    private readonly IValidator<BatchURLCreateDTO> _batchURLValidator = batchValidator;
    private readonly IValidator<GetPagedItemsRequest> _getPagedItemsRequestValidator = getPagedItemsRequestValidator;

    [Authorize(Policy = "AllUsers")]
    [HttpGet("/{id:int}")]
    [EnableRateLimiting("FetchData")]
    public async Task<IActionResult> GetURL([FromRoute] int id)
    {
        URLDTO result = await _urlService.GetURLAsync(id);
        var response = CreateResult.Success(result);
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpGet("Profile/URLTable")]
    [EnableRateLimiting("DataFetch")]
    public async Task<ActionResult<PagedResult<URLDTO>>> GetUserURLs([FromQuery] GetPagedItemsRequest reqInfo)
    {
        await _getPagedItemsRequestValidator.ValidateAndThrowAsync(reqInfo);
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        PagedResult<URLDTO> result = await _urlService.GetPagedURLsAsync(userId, reqInfo.PageNumber, reqInfo.PageSize);
        var response = CreateResult.Paged(result);
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("AddURL")]
    [EnableRateLimiting("AddURL")]
    public async Task<IActionResult> AddURL([FromBody] URLCreateDTO createDTO)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _validator.ValidateAndThrowAsync(createDTO);
        URLShortenResponse result = await _urlService.AddURL(createDTO, userId);
        var response = CreateResult.Success(result);
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("AddBatchURL")]
    [EnableRateLimiting("AddURL")]
    public async Task<IActionResult> AddBatchURL([FromBody] BatchURLCreateDTO createDTO)
    {
        await _batchURLValidator.ValidateAndThrowAsync(createDTO);
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _urlService.AddBatchURL(createDTO, userId!);
        var response = CreateResult.Success(result);
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("ToggleActivation/{id:long}")]
    [EnableRateLimiting("FetchData")]
    public async Task<IActionResult> ToggleActivation([FromRoute] long id)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _urlService.ToggleStateAsync(id, x => x.IsActive = !x.IsActive, userId);
        var response = CreateResult.Success();
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("ToggleMonetization/{id:long}")]
    [EnableRateLimiting("FetchData")]
    public async Task<IActionResult> ToggleMonetization([FromRoute] long id)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _urlService.ToggleStateAsync(id, x => x.IsMonetized = !x.IsMonetized, userId);
        var response = CreateResult.Success();
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpDelete("Delete/{id:long}")]
    [EnableRateLimiting("Deletion")]
    public async Task<IActionResult> DeleteURL([FromRoute] long id)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _urlService.DeleteURL(id, userId);
        var response = CreateResult.Success();
        return Ok(response);
    }
}
