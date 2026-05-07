using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using URLShortener.Application.Common.Models.Identity;
using URLShortener.Application.Configuration;
using URLShortener.Application.Features.Users.DTOs;
using URLShortener.Application.Features.Users.Interfaces.Services;
using URLShortenerAPI.Utility;

namespace URLShortenerAPI.Controllers;

[ApiController]
[Route("/api/[Controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<UserUpdateDTO> _userUpdateValidator;
    private readonly IValidator<ChangeEmailRequest> _emailValidator;
    private readonly IUserStatsService _userStatsService;
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AuthenticationCookieSettings _authenticationCookieSettings;
    private readonly JwtSettings _jwtSettings;

    public UsersController(
        IUserService userService,
        IValidator<UserUpdateDTO> userUpdateValidator,
        IValidator<ChangeEmailRequest> emailValidator,
        IUserStatsService userStatsService,
        UserManager<AppIdentityUser> userManager,
        ITokenService tokenService,
        IOptions<AuthenticationCookieSettings> authenticationCookieSettings,
        IOptions<JwtSettings> jwtSettings)
    {
        _userService = userService;
        _userUpdateValidator = userUpdateValidator;
        _emailValidator = emailValidator;
        _userStatsService = userStatsService;
        _userManager = userManager;
        _tokenService = tokenService;
        _authenticationCookieSettings = authenticationCookieSettings.Value;
        _jwtSettings = jwtSettings.Value;
    }

    [Authorize(Policy = "AllUsers")]
    [HttpGet]
    [EnableRateLimiting("DataFetch")]
    public async Task<IActionResult> GetUserById()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        UserDTO result = await _userService.GetUserByIDAsync(userId);
        var response = CreateResult.Success(result);
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpGet("Profile")]
    [EnableRateLimiting("DataFetch")]
    public async Task<IActionResult> GetStats()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        UserStats result = await _userStatsService.GetUserStats(userId);
        var response = CreateResult.Success(result);
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpGet("Dashboard")]
    [EnableRateLimiting("DataFetch")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        UserDashboardDTO result = await _userStatsService.GetDashboardByIDAsync(userId);
        var response = CreateResult.Success(result);
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("ChangeEmail")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest reqInfo)
    {
        await _emailValidator.ValidateAndThrowAsync(reqInfo);
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        UserDTO result = await _userService.SetNewEmailAsync(reqInfo.NewEmail, userId);
        var response = CreateResult.Success(result);
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPut("UpdateUser")]
    [EnableRateLimiting("UpdateUser")]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDTO newUser)
    {
        await _userUpdateValidator.ValidateAndThrowAsync(newUser);

        // extract user identity from JWT claims
        long userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        string userName = HttpContext.User.Identity?.Name!;

        UserDTO result = await _userService.UpdateUserInfoAsync(newUser, userId);

        // if username changed, the current JWT is stale, I issue a fresh one
        if (!userName.Equals(result.Username, StringComparison.Ordinal))
            await ReAuthenticateUser(userId);

        var response = CreateResult.Success(result);
        return Ok(response);
    }

    private async Task ReAuthenticateUser(long userId)
    {
        // fetch the updated Identity user to build claims from the latest state
        var identityUser = await _userManager.FindByIdAsync(userId.ToString());
        if (identityUser is null) return;

        // generate a new JWT and overwrite the existing cookie
        var jwToken = await _tokenService.GenerateJWTokenAsync(identityUser);
        var jwtCookieOptions = CookieOptionsFactory.CreateJwtCookieOptions(_authenticationCookieSettings, _jwtSettings);
        Response.Cookies.Append(_authenticationCookieSettings.JwtCookieName, jwToken, jwtCookieOptions);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("Delete/{id:long}")]
    [EnableRateLimiting("UpdateUser")]
    public async Task<IActionResult> DeleteUser([FromRoute] long id)
    {
        await _userService.DeleteUserAsync(id);
        var response = CreateResult.Success();
        return Ok(response);
    }
}
