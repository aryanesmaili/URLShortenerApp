using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SharedDataModels.Responses;
using System.Security.Claims;
using System.Text.Json;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Interfaces.Services.User;
using URLShortener.Application.Models;
using URLShortener.Application.Utility.Exceptions;

namespace URLShortenerAPI.Controllers;

public sealed class AuthController(
    IAuthenticationService authenticationService,
    IValidator<UserLoginDTO> loginValidator,
    IValidator<ChangeEmailRequest> emailValidator,
    IValidator<ChangePasswordRequest> changePasswordValidator,
    IAntiforgery antiForgery,
    IValidator<UserCreateDTO> userValidator,
    UserManager<AppIdentityUser> userManager,
    SignInManager<AppIdentityUser> signInManager,
    ITokenService tokenService,
    IValidator<CheckResetEmailCodeRequest> checkEmailCodeValidator,
    IOptions<AuthenticationCookieSettings> authenticationCookieSettings,
    IOptions<AntiforgerySettings> antiforgerySettings,
    IOptions<JwtSettings> jwtSettings) : ControllerBase
{
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly IValidator<UserCreateDTO> _userValidator = userValidator;
    private readonly IValidator<UserLoginDTO> _userLoginValidator = loginValidator;
    private readonly IValidator<ChangeEmailRequest> _emailValidator = emailValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator = changePasswordValidator;
    private readonly IValidator<CheckResetEmailCodeRequest> _checkEmailCodeValidator = checkEmailCodeValidator; // TODO: Implement This
    private readonly IAntiforgery _antiForgery = antiForgery;
    private readonly UserManager<AppIdentityUser> _userManager = userManager;
    private readonly SignInManager<AppIdentityUser> _signInManager = signInManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly AuthenticationCookieSettings _authenticationCookieSettings = authenticationCookieSettings.Value;
    private readonly AntiforgerySettings _antiforgerySettings = antiforgerySettings.Value;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    // TODO: in the end of refactoring Check if we still need this endpoint
    [Authorize(Policy = "AllUsers")]
    [HttpGet("antiforgery/token")]
    [EnableRateLimiting("Auth")]
    [IgnoreAntiforgeryToken]
    public IActionResult GetForgeryToken()
    {
        APIResponse<string> response;
        try
        {
            var tokens = _antiForgery.GetAndStoreTokens(HttpContext);
            response = new()
            { Success = true, Result = tokens.RequestToken };
            return Ok(response);
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

    [IgnoreAntiforgeryToken]
    [HttpPost("Captcha")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> VerifyCaptcha([FromBody] string token)
    {
        APIResponse<CaptchaVerificationResponse> response;
        try
        {
            string IPAddress = HttpContext.Connection.RemoteIpAddress!.MapToIPv4().ToString();
            CaptchaVerificationResponse result = await _authenticationService.VerifyCaptcha(token, IPAddress);

            response = new()
            { Success = result.Success, Result = result };
            return Ok(response);
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

    [IgnoreAntiforgeryToken]
    [HttpPost("Login")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO LoginInfo)
    {
        APIResponse<UserDTO> response;
        try
        {
            await _userLoginValidator.ValidateAndThrowAsync(LoginInfo);
            var loginResult = await _authenticationService.LoginAsync(LoginInfo);
            if (!loginResult.Success)
            {
                response = new() { ErrorMessage = loginResult.ErrorMessage, ErrorType = loginResult.ErrorType };
                return BadRequest(response);
            }
            SetAuthCookies(loginResult.Result!.JWToken, loginResult.Result.RefreshToken.Token);
            response = new()
            { Result = loginResult.Result.User, Success = true };
            return Ok(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }
        catch (ArgumentException e)
        {
            response = new() { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
            return BadRequest(response);
        }
        catch (ValidationException e)
        {
            List<string> errors = [];

            foreach (var error in e.Errors)
                errors.Add($"{error.PropertyName}: {error.ErrorMessage}");

            response = new() { ErrorType = ErrorType.ValidationException, ErrorMessage = e.Message, Errors = errors };

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

    private void SetAuthCookies(string jwt, string refreshToken)
    {
        var refreshCookieOptions = CookieOptionsFactory.CreateRefreshCookieOptions(_authenticationCookieSettings);
        var jwtCookieOptions = CookieOptionsFactory.CreateJwtCookieOptions(_authenticationCookieSettings, _jwtSettings);

        Response.Cookies.Append(_authenticationCookieSettings.RefreshTokenCookieName, JsonSerializer.Serialize(refreshToken), refreshCookieOptions);
        Response.Cookies.Append(_authenticationCookieSettings.JwtCookieName, jwt, jwtCookieOptions);
    }

    [IgnoreAntiforgeryToken]
    [HttpPost("Register")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Register([FromBody] UserCreateDTO userCreateDTO)
    {
        APIResponse<UserDTO> response;
        try
        {
            await _userValidator.ValidateAndThrowAsync(userCreateDTO);

            UserDTO result = await _authenticationService.RegisterUserAsync(userCreateDTO);
            response = new()
            { Result = result, Success = true };
            return Ok(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }
        catch (ArgumentException e)
        {
            response = new() { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
            return BadRequest(response);
        }
        catch (ValidationException e)
        {
            List<string> errors = [];

            foreach (var error in e.Errors)
                errors.Add($"{error.PropertyName}: {error.ErrorMessage}");

            response = new() { ErrorType = ErrorType.ValidationException, ErrorMessage = e.Message, Errors = errors };

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

    [IgnoreAntiforgeryToken]
    [HttpPost("RequestResetPassword")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> RequestResetPassword([FromBody] ChangePasswordRequest reqInfo)
    {
        APIResponse<string> response;
        try
        {
            await _changePasswordValidator.ValidateAndThrowAsync(reqInfo);

            await _authenticationService.RequestPasswordResetAsync(reqInfo);
            response = new()
            { Result = string.Empty, Success = true };
            return Ok(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }
        catch (ArgumentException e)
        {
            response = new() { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
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

    [IgnoreAntiforgeryToken]
    [HttpPost("ResetPassword")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> ResetPassword([FromBody] CheckResetPasswordCodeRequest reqInfo)
    {
        APIResponse<string> response;
        try
        {
            await _authenticationService.ResetPasswordAsync(reqInfo.Email, reqInfo.Token, reqInfo.NewPassword);
            response = new()
            { Result = string.Empty, Success = true };
            return Ok(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }
        catch (ArgumentException e)
        {
            response = new() { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
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
    [HttpPost("ResetEmail")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> ResetEmail([FromBody] ChangeEmailRequest request)
    {
        APIResponse<string> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _emailValidator.ValidateAndThrowAsync(request);

            await _authenticationService.RequestEmailChangeAsync(userId, request.NewEmail);

            response = new()
            { Success = true, Result = string.Empty };
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            response = new() { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
            return BadRequest(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }
        catch (NotAuthorizedException e)
        {
            response = new()
            { ErrorType = ErrorType.NotAuthorizedException, ErrorMessage = e.Message };
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
    [HttpPost("CheckEmailResetCode")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> CheckEmailResetCode([FromQuery] CheckResetEmailCodeRequest reqInfo)
    {
        APIResponse<string> response;
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _checkEmailCodeValidator.ValidateAndThrowAsync(reqInfo);

            await _authenticationService.ConfirmEmailChangeAsync(userId, reqInfo.NewEmail, reqInfo.Token);
            response = new()
            { Success = true, Result = string.Empty };
            return Ok(response);
        }
        catch (ArgumentException e)
        {
            response = new() { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
            return BadRequest(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }
        catch (NotAuthorizedException e)
        {
            response = new()
            { ErrorType = ErrorType.NotAuthorizedException, ErrorMessage = e.Message };
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
    [HttpPost("Logout")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Logout()
    {
        APIResponse<string> response;
        // Retrieve the refresh token from the cookies
        if (!Request.Cookies.TryGetValue(_authenticationCookieSettings.RefreshTokenCookieName, out var refreshTokenJson))
            return BadRequest("No refresh token found in cookies.");

        try
        {
            RefreshTokenDTO? refreshToken = JsonSerializer.Deserialize<RefreshTokenDTO>(refreshTokenJson);
            // Invalidate the refresh token in the database
            await _authenticationService.RevokeTokenAsync(refreshToken!.Token);

            // Remove the cookie
            var deleteOptions = CookieOptionsFactory.CreateDeletionOptions(_authenticationCookieSettings);
            Response.Cookies.Delete(_authenticationCookieSettings.RefreshTokenCookieName, deleteOptions);
            Response.Cookies.Delete(_authenticationCookieSettings.JwtCookieName, deleteOptions);
            Response.Cookies.Delete(_antiforgerySettings.CookieName, new CookieOptions { Path = _antiforgerySettings.CookiePath });

            response = new()
            { Success = true, Result = string.Empty };
            return Ok(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }
        catch (ArgumentException e)
        {
            response = new() { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
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

    [IgnoreAntiforgeryToken]
    [HttpPost("RefreshToken")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> RefreshToken()
    {
        APIResponse<string> response;
        if (!Request.Cookies.TryGetValue(_authenticationCookieSettings.RefreshTokenCookieName, out var refreshTokenJson))
        {
            response = new() { Success = false, ErrorMessage = "RefreshToken Not Found" };
            return BadRequest(response);
        }
        try
        {
            RefreshTokenDTO? refrehToken = JsonSerializer.Deserialize<RefreshTokenDTO>(refreshTokenJson);
            var result = await _authenticationService.TokenRefresher(refrehToken!.Token);

            SetAuthCookies(result.jwt, result.refreshToken);

            response = new()
            { Result = string.Empty, Success = true };
            return Ok(response);
        }
        catch (NotFoundException e)
        {
            response = new() { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
            return NotFound(response);
        }
        catch (ArgumentException e)
        {
            response = new() { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
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
