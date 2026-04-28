using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SharedDataModels.Responses;
using System.Security.Claims;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Interfaces.Services.User;
using URLShortener.Application.Models;
using URLShortener.Application.Utility.Exceptions;

namespace URLShortenerAPI.Controllers;

[ApiController]
[Route("/api/[Controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<UserUpdateDTO> _userUpdateValidator;
    private readonly IValidator<ChangeEmailRequest> _emailValidator;
    private readonly IUserStatsService _userStatsService;
    private readonly IAuthenticationService _authenticationService;
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AuthenticationCookieSettings _authenticationCookieSettings;
    private readonly JwtSettings _jwtSettings;

    public UsersController(
        IUserService userService,
        IValidator<UserUpdateDTO> userUpdateValidator,
        IValidator<ChangeEmailRequest> emailValidator,
        IUserStatsService userStatsService,
        IAuthenticationService authenticationService,
        UserManager<AppIdentityUser> userManager,
        ITokenService tokenService,
        IOptions<AuthenticationCookieSettings> authenticationCookieSettings,
        IOptions<JwtSettings> jwtSettings)
    {
        _userService = userService;
        _userUpdateValidator = userUpdateValidator;
        _emailValidator = emailValidator;
        _userStatsService = userStatsService;
        _authenticationService = authenticationService;
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
        APIResponse<UserDTO> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            UserDTO result = await _userService.GetUserByIDAsync(userId);
            response = new()
            { Result = result, Success = true };
            return Ok(response);
        }
        catch (NotFoundException e)
        {
            response = new()
            { ErrorType = ErrorType.NotFound, Message = e.Message };
            return NotFound(response);
        }
        catch (Exception e)
        {
            DebugErrorResponse errorResponse = new()
            {
                Message = e.Message,
                InnerException = e.InnerException?.ToString() ?? "",
                StackTrace = e.StackTrace ?? ""
            };
            return StatusCode(500, errorResponse);
        }
    }

    [Authorize(Policy = "AllUsers")]
    [HttpGet("Profile")]
    [EnableRateLimiting("DataFetch")]
    public async Task<IActionResult> GetStats()
    {
        APIResponse<UserStats> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            UserStats result = await _userStatsService.GetUserStats(userId);

            response = new()
            { Result = result, Success = true };
            return Ok(response);
        }
        catch (NotFoundException e)
        {
            response = new()
            { ErrorType = ErrorType.NotFound, Message = e.Message };
            return NotFound(response);
        }
        catch (ArgumentException e)
        {
            response = new()
            { ErrorType = ErrorType.Argument, Message = e.Message };
            return BadRequest(response);
        }
        catch (NotAuthorizedException e)
        {
            response = new()
            { ErrorType = ErrorType.Unauthorized, Message = e.Message };
            return BadRequest(response);
        }
        catch (Exception e)
        {
            DebugErrorResponse errorResponse = new()
            {
                Message = e.Message,
                InnerException = e.InnerException?.ToString() ?? "",
                StackTrace = e.StackTrace?.ToString() ?? ""
            };
            return StatusCode(500, errorResponse);
        }
    }

    [Authorize(Policy = "AllUsers")]
    [HttpGet("Dashboard")]
    [EnableRateLimiting("DataFetch")]
    public async Task<IActionResult> GetDashboard()
    {
        APIResponse<UserDashboardDTO> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            UserDashboardDTO result = await _userStatsService.GetDashboardByIDAsync(userId);
            response = new()
            { Result = result, Success = true };
            return Ok(response);
        }
        catch (NotFoundException e)
        {
            response = new()
            { ErrorType = ErrorType.NotFound, Message = e.Message };
            return NotFound(response);
        }
        catch (NotAuthorizedException e)
        {
            response = new()
            { ErrorType = ErrorType.Unauthorized, Message = e.Message };
            return BadRequest(response);
        }
        catch (Exception e)
        {
            DebugErrorResponse errorResponse = new()
            {
                Message = e.Message,
                InnerException = e.InnerException?.ToString() ?? "",
                StackTrace = e.StackTrace ?? ""
            };
            return StatusCode(500, errorResponse);
        }
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("ChangeEmail/{id:int}")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> ChangeEmail(int id, [FromBody] ChangeEmailRequest reqInfo)
    {
        APIResponse<UserDTO> response;
        try
        {
            await _emailValidator.ValidateAndThrowAsync(reqInfo);

            UserDTO result = await _userService.SetNewEmailAsync(reqInfo.NewEmail, id);
            response = new()
            { Success = true, Result = result };
            return Ok(response);
        }
        catch (ValidationException e)
        {
            List<string> errors = [];

            foreach (var error in e.Errors)
                errors.Add($"{error.PropertyName}: {error.ErrorMessage}");

            response = new() { ErrorType = ErrorType.Validation, Message = e.Message, Errors = errors };

            return BadRequest(response);
        }
        catch (ArgumentException e)
        {
            response = new() { ErrorType = ErrorType.Argument, Message = e.Message };
            return BadRequest(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, Message = e.Message };
            return NotFound(response);
        }
        catch (NotAuthorizedException e)
        {
            response = new()
            { ErrorType = ErrorType.Unauthorized, Message = e.Message };
            return BadRequest(response);
        }
        catch (Exception e)
        {
            var errorResponse = new DebugErrorResponse
            {
                Message = e.Message,
                InnerException = e.InnerException?.ToString() ?? "",
                StackTrace = e.StackTrace ?? ""
            };
            return StatusCode(500, errorResponse);
        }
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPut("UpdateUser")]
    [EnableRateLimiting("UpdateUser")]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDTO newUser)
    {
        APIResponse<UserDTO> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userName = HttpContext.User.Identity?.Name!;
        try
        {
            await _userUpdateValidator.ValidateAndThrowAsync(newUser);
            UserDTO result = await _userService.UpdateUserInfoAsync(newUser, userId);

            // if the user's username has changed, we generate them a new JWT
            if (!userName.Equals(result.Username, StringComparison.Ordinal))
            {
                // get identity user id from claims (the NameIdentifier claim is the identity user id)
                var identityId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var identityUser = await _userManager.FindByIdAsync(identityId.ToString());
                if (identityUser != null && !string.Equals(identityUser.UserName, result.Username, StringComparison.Ordinal))
                {
                    identityUser.UserName = result.Username;
                    var updateResult = await _userManager.UpdateAsync(identityUser);
                    if (!updateResult.Succeeded)
                        throw new ArgumentException("Failed to update identity username");

                    // generate fresh JWT using TokenService that builds claims from Identity
                    var jwToken = await _tokenService.GenerateJWTokenAsync(identityUser);
                    var jwtCookieOptions = CookieOptionsFactory.CreateJwtCookieOptions(_authenticationCookieSettings, _jwtSettings);
                    Response.Cookies.Append(_authenticationCookieSettings.JwtCookieName, jwToken, jwtCookieOptions);
                }
            }

            response = new()
            { Success = true, Result = result };
            return Ok(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, Message = e.Message };
            return NotFound(response);
        }
        catch (ArgumentException e)
        {
            response = new() { ErrorType = ErrorType.Argument, Message = e.Message };
            return BadRequest(response);
        }
        catch (ValidationException e)
        {
            List<string> errors = [];

            foreach (var error in e.Errors)
                errors.Add($"{error.PropertyName + ":"} {error.ErrorMessage}");

            response = new() { ErrorType = ErrorType.Validation, Message = e.Message, Errors = errors };

            return BadRequest(response);
        }
        catch (Exception e)
        {
            var errorResponse = new DebugErrorResponse
            {
                Message = e.Message,
                InnerException = e.InnerException?.ToString() ?? "",
                StackTrace = e.StackTrace ?? ""
            };
            return StatusCode(500, errorResponse);
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("Delete/{id:int}")]
    [EnableRateLimiting("UpdateUser")]
    public async Task<IActionResult> DeleteUser([FromRoute] int id)
    {
        APIResponse<string> response;
        try
        {
            await _userService.DeleteUserAsync(id);
            response = new()
            { Success = true };
            return Ok(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, Message = e.Message };
            return NotFound(response);
        }
        catch (ArgumentException e)
        {
            response = new() { ErrorType = ErrorType.Argument, Message = e.Message };
            return BadRequest(response);
        }
        catch (Exception e)
        {
            var errorResponse = new DebugErrorResponse
            {
                Message = e.Message,
                InnerException = e.InnerException?.ToString() ?? "",
                StackTrace = e.StackTrace ?? ""
            };
            return StatusCode(500, errorResponse);
        }
    }
}
