using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
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
using URLShortenerAPI.Utility;

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
    IOptions<JwtSettings> jwtSettings,
    IValidator<VerifyCaptchaRequest> verifyCaptchaValidator,
    IValidator<CheckResetPasswordCodeRequest> checkResetPasswordCodeValidator) : ControllerBase
{
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly IValidator<UserCreateDTO> _userValidator = userValidator;
    private readonly IValidator<UserLoginDTO> _userLoginValidator = loginValidator;
    private readonly IValidator<ChangeEmailRequest> _emailValidator = emailValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator = changePasswordValidator;
    private readonly IValidator<CheckResetEmailCodeRequest> _checkEmailCodeValidator = checkEmailCodeValidator; // TODO: Implement This
    private readonly IValidator<VerifyCaptchaRequest> _verifyCaptchaValidator = verifyCaptchaValidator; // TODO: Implement This
    private readonly IValidator<CheckResetPasswordCodeRequest> _checkResetPasswordCodeValidator = checkResetPasswordCodeValidator; // TODO: Implement This
    private readonly IAntiforgery _antiForgery = antiForgery;
    private readonly UserManager<AppIdentityUser> _userManager = userManager;
    private readonly SignInManager<AppIdentityUser> _signInManager = signInManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly AuthenticationCookieSettings _authenticationCookieSettings = authenticationCookieSettings.Value;
    private readonly AntiforgerySettings _antiforgerySettings = antiforgerySettings.Value;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    [Authorize(Policy = "AllUsers")]
    [HttpGet("antiforgery/token")]
    [EnableRateLimiting("Auth")]
    [IgnoreAntiforgeryToken]
    public IActionResult GetForgeryToken()
    {
        var tokens = _antiForgery.GetAndStoreTokens(HttpContext);
        var response = CreateResult.Success(tokens.RequestToken);
        return Ok(response);
    }

    [IgnoreAntiforgeryToken]
    [HttpPost("Captcha")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> VerifyCaptcha([FromBody] VerifyCaptchaRequest reqInfo)
    {
        await _verifyCaptchaValidator.ValidateAndThrowAsync(reqInfo);
        string IPAddress = HttpContext.Connection.RemoteIpAddress!.MapToIPv4().ToString();
        CaptchaVerificationResponse result = await _authenticationService.VerifyCaptcha(reqInfo.Token, IPAddress);
        var respnse = CreateResult.Success(result);
        return Ok(respnse);
    }

    [IgnoreAntiforgeryToken]
    [HttpPost("Login")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO LoginInfo)
    {
        await _userLoginValidator.ValidateAndThrowAsync(LoginInfo);
        var loginResult = await _authenticationService.LoginAsync(LoginInfo);
        SetAuthCookies(loginResult.JWToken, loginResult.RefreshToken.Token);
        var response = CreateResult.Success(loginResult.User);
        return Ok(response);
    }

    private void SetAuthCookies(string jwt, string refreshToken)
    {
        var refreshCookieOptions = CookieOptionsFactory.CreateRefreshCookieOptions(_authenticationCookieSettings);
        var jwtCookieOptions = CookieOptionsFactory.CreateJwtCookieOptions(_authenticationCookieSettings, _jwtSettings);

        Response.Cookies.Append(_authenticationCookieSettings.RefreshTokenCookieName, refreshToken, refreshCookieOptions);
        Response.Cookies.Append(_authenticationCookieSettings.JwtCookieName, jwt, jwtCookieOptions);
    }

    [IgnoreAntiforgeryToken]
    [HttpPost("Register")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Register([FromBody] UserCreateDTO userCreateDTO)
    {
        await _userValidator.ValidateAndThrowAsync(userCreateDTO);
        UserDTO result = await _authenticationService.RegisterUserAsync(userCreateDTO);
        var response = CreateResult.Success(result);
        return Ok(response);
    }

    [IgnoreAntiforgeryToken]
    [HttpPost("RequestResetPassword")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> RequestResetPassword([FromBody] ChangePasswordRequest reqInfo)
    {
        await _changePasswordValidator.ValidateAndThrowAsync(reqInfo);
        await _authenticationService.RequestPasswordResetAsync(reqInfo);
        var response = CreateResult.Success();
        return Ok(response);
    }

    [IgnoreAntiforgeryToken]
    [HttpPost("ResetPassword")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> ResetPassword([FromBody] CheckResetPasswordCodeRequest reqInfo)
    {
        await _checkResetPasswordCodeValidator.ValidateAndThrowAsync(reqInfo);
        await _authenticationService.ResetPasswordAsync(reqInfo.Email, reqInfo.Token, reqInfo.NewPassword);
        var response = CreateResult.Success();
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("ResetEmail")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> ResetEmail([FromBody] ChangeEmailRequest request)
    {
        await _emailValidator.ValidateAndThrowAsync(request);
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authenticationService.RequestEmailChangeAsync(userId, request.NewEmail);
        var response = CreateResult.Success();
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("CheckEmailResetCode")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> CheckEmailResetCode([FromQuery] CheckResetEmailCodeRequest reqInfo)
    {
        await _checkEmailCodeValidator.ValidateAndThrowAsync(reqInfo);
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authenticationService.ConfirmEmailChangeAsync(userId, reqInfo.NewEmail, reqInfo.Token);
        var response = CreateResult.Success();
        return Ok(response);
    }

    [Authorize(Policy = "AllUsers")]
    [HttpPost("Logout")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Logout()
    {
        // Retrieve the refresh token from the cookies
        if (!Request.Cookies.TryGetValue(_authenticationCookieSettings.RefreshTokenCookieName, out var refreshToken))
            throw new ArgumentException("No refresh token found in cookies.");

        // Invalidate the refresh token in the database
        await _authenticationService.RevokeTokenAsync(refreshToken);

        // Remove the cookie
        var deleteOptions = CookieOptionsFactory.CreateDeletionOptions(_authenticationCookieSettings);
        Response.Cookies.Delete(_authenticationCookieSettings.RefreshTokenCookieName, deleteOptions);
        Response.Cookies.Delete(_authenticationCookieSettings.JwtCookieName, deleteOptions);
        Response.Cookies.Delete(_antiforgerySettings.CookieName, new CookieOptions { Path = _antiforgerySettings.CookiePath });

        var response = CreateResult.Success();
        return Ok(response);
    }

    [IgnoreAntiforgeryToken]
    [HttpPost("RefreshToken")]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> RefreshToken()
    {
        if (!Request.Cookies.TryGetValue(_authenticationCookieSettings.RefreshTokenCookieName, out var refreshToken))
            throw new ArgumentException("No refresh token found in cookies.");

        var result = await _authenticationService.TokenRefresher(refreshToken);
        SetAuthCookies(result.jwt, result.refreshToken);
        var response = CreateResult.Success();
        return Ok(response);
    }
}
