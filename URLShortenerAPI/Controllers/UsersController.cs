﻿using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedDataModels.CustomClasses;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using System.Security.Claims;
using System.Text.Json;
using URLShortenerAPI.Services.Interfaces;
using URLShortenerAPI.Utility.Exceptions;

namespace URLShortenerAPI.Controllers
{
    [ApiController]
    [Route("/api/[Controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<UserCreateDTO> _userValidator;
        private readonly IValidator<UserUpdateDTO> _userUpdateValidator;
        private readonly IValidator<UserLoginDTO> _userLoginValidator;
        private readonly IValidator<ChangeEmailRequest> _emailValidator;
        private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public UsersController(IUserService userService,
            IValidator<UserCreateDTO> userValidator,
            IValidator<UserLoginDTO> userLoginValidator,
            IValidator<UserUpdateDTO> userUpdateValidator,
            IValidator<ChangeEmailRequest> emailValidator,
            IValidator<ChangePasswordRequest> changePasswordValidator,
            IWebHostEnvironment webHostEnvironment)
        {
            _userService = userService;
            _userValidator = userValidator;
            _userLoginValidator = userLoginValidator;
            _userUpdateValidator = userUpdateValidator;
            _emailValidator = emailValidator;
            _changePasswordValidator = changePasswordValidator;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize("AllUsers")]
        [HttpGet("Profile/{userId}")]
        public async Task<ActionResult<PagedResult<URLDTO>>> GetUserURLs(int userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            APIResponse<PagedResult<URLDTO>> response;
            var username = HttpContext.User.Identity?.Name;
            try
            {
                if (pageNumber < 1)
                    throw new ArgumentException("Page number must be greater than or equal to 1.");

                else if (pageSize < 1)
                    throw new ArgumentException("Page size must be greater than or equal to 1.");

                PagedResult<URLDTO> result = await _userService.GetPagedResult(userId, pageNumber, pageSize, username!);

                if (result.Items.Count == 0)
                    throw new NotFoundException("No URLs found for the specified user.");
                response = new()
                { Result = result, Success = true };
                return Ok(response);
            }
            catch (NotFoundException e)
            {
                response = new()
                { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
                return NotFound(response);
            }
            catch (ArgumentException e)
            {
                response = new()
                { ErrorType = ErrorType.ArgumentException, ErrorMessage = e.Message };
                return BadRequest(response);
            }
            catch (NotAuthorizedException e)
            {
                response = new()
                { ErrorType = ErrorType.NotAuthorizedException, ErrorMessage = e.Message };
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
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            APIResponse<UserDTO> response;
            try
            {
                UserDTO result = await _userService.GetUserByIDAsync(id);
                response = new()
                { Result = result, Success = true };
                return Ok(response);
            }
            catch (NotFoundException e)
            {
                response = new()
                { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
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
        [HttpGet("Dashboard/{id:int}")]
        public async Task<IActionResult> GetDashboard([FromRoute] int id)
        {
            APIResponse<UserDashboardDTO> response;
            var username = HttpContext.User.Identity?.Name;
            try
            {
                UserDashboardDTO result = await _userService.GetDashboardByIDAsync(id, username!);
                response = new()
                { Result = result, Success = true };
                return Ok(response);
            }
            catch (NotFoundException e)
            {
                response = new()
                { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
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
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsername([FromRoute] string username)
        {
            APIResponse<UserDTO> response;
            try
            {
                UserDTO result = await _userService.GetUserByUsernameAsync(username);
                response = new()
                { Result = result, Success = true };
                return Ok(response);
            }
            catch (NotFoundException e)
            {
                response = new()
                { ErrorType = ErrorType.NotFound, ErrorMessage = e.Message };
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

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO LoginInfo)
        {
            APIResponse<UserDTO> response;
            try
            {
                await _userLoginValidator.ValidateAndThrowAsync(LoginInfo);

                UserLoginResponse result = await _userService.LoginUserAsync(LoginInfo);

                CookieOptions refreshCookieOptions = new()
                {
                    HttpOnly = true, // Prevents access from JavaScript
                    Expires = DateTime.UtcNow.AddDays(7), // Set expiry for refresh token
                    SameSite = _webHostEnvironment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Lax, // Prevents CSRF attacks
                    Secure = !_webHostEnvironment.IsDevelopment() // Use HTTPS
                };
                CookieOptions jwtCookieOptions = new()
                {
                    HttpOnly = true, // Prevents access from JavaScript
                    Expires = DateTime.UtcNow.AddMinutes(30), // Set expiry for refresh token
                    SameSite = _webHostEnvironment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict,
                    Secure = !_webHostEnvironment.IsDevelopment()
                };

                Response.Cookies.Append("refreshToken", JsonSerializer.Serialize(result.RefreshToken), refreshCookieOptions);
                Response.Cookies.Append("jwt", result.JWToken, jwtCookieOptions);

                response = new()
                { Result = result.User, Success = true };
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
                {
                    errors.Add($"{error.PropertyName}: {error.ErrorMessage}");
                }
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

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserCreateDTO userCreateDTO)
        {
            APIResponse<UserDTO> response;
            try
            {
                await _userValidator.ValidateAndThrowAsync(userCreateDTO);

                UserDTO result = await _userService.RegisterUserAsync(userCreateDTO);
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
                {
                    errors.Add($"{error.PropertyName}: {error.ErrorMessage}");
                }
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

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] string identifier)
        {
            APIResponse<string> response;
            try
            {
                await _userService.ResetPasswordAsync(identifier);
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

        [HttpPost("CheckResetCode")]
        public async Task<IActionResult> CheckPasswordResetCode([FromBody] CheckVerificationCode reqInfo)
        {
            APIResponse<UserDTO> response;
            try
            {
                UserLoginResponse result = await _userService.CheckPasswordResetCodeAsync(reqInfo.Identifier, reqInfo.Code);

                CookieOptions refreshCookieOptions = new()
                {
                    HttpOnly = true, // Prevents access from JavaScript
                    Expires = DateTime.UtcNow.AddDays(7), // Set expiry for refresh token
                    SameSite = _webHostEnvironment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Lax, // Prevents CSRF attacks
                    Secure = !_webHostEnvironment.IsDevelopment() // Use HTTPS
                };
                CookieOptions jwtCookieOptions = new()
                {
                    HttpOnly = true, // Prevents access from JavaScript
                    Expires = DateTime.UtcNow.AddMinutes(30), // Set expiry for refresh token
                    SameSite = _webHostEnvironment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict,
                    Secure = !_webHostEnvironment.IsDevelopment()
                };

                Response.Cookies.Append("refreshToken", JsonSerializer.Serialize(result.RefreshToken), refreshCookieOptions);
                Response.Cookies.Append("jwt", result.JWToken, jwtCookieOptions);

                response = new()
                { Result = result.User, Success = true };
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
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest reqInfo)
        {
            APIResponse<UserDTO> response;
            var username = HttpContext.User.Identity?.Name;
            try
            {
                await _changePasswordValidator.ValidateAndThrowAsync(reqInfo);
                UserDTO result = await _userService.ChangePasswordAsync(reqInfo, username!);

                response = new()
                { Result = result, Success = true };
                return Ok(response);
            }

            catch (ValidationException e)
            {
                List<string> errors = [];

                foreach (var error in e.Errors)
                {
                    errors.Add($"{error.PropertyName}: {error.ErrorMessage}");
                }
                response = new() { ErrorType = ErrorType.ValidationException, ErrorMessage = e.Message, Errors = errors };

                return BadRequest(response);
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
        [HttpPost("ResetEmail/{id:int}")]
        public async Task<IActionResult> ResetEmail(int id)
        {
            APIResponse<string> response;
            var username = HttpContext.User.Identity?.Name;
            try
            {
                await _userService.ResetEmailAsync(id, username!);
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
        [HttpPost("CheckEmailResetCode/{id:int}")]
        public async Task<IActionResult> CheckEmailResetCode(int id, [FromBody] CheckVerificationCode reqInfo)
        {
            APIResponse<string> response;
            var username = HttpContext.User.Identity?.Name;
            try
            {
                ArgumentException.ThrowIfNullOrEmpty(reqInfo.Code);

                await _userService.CheckEmailResetCodeAsync(reqInfo.Code, id, username!);
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
        [HttpPost("ChangeEmail/{id:int}")]
        public async Task<IActionResult> ChangeEmail(int id, [FromBody] ChangeEmailRequest reqInfo)
        {
            APIResponse<UserDTO> response;
            var username = HttpContext.User.Identity?.Name;
            try
            {
                await _emailValidator.ValidateAndThrowAsync(reqInfo);

                UserDTO result = await _userService.SetNewEmailAsync(reqInfo.NewEmail, id, username!);
                response = new()
                { Success = true, Result = result };
                return Ok(response);
            }
            catch (ValidationException e)
            {
                List<string> errors = [];

                foreach (var error in e.Errors)
                {
                    errors.Add($"{error.PropertyName}: {error.ErrorMessage}");
                }
                response = new() { ErrorType = ErrorType.ValidationException, ErrorMessage = e.Message, Errors = errors };

                return BadRequest(response);
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
        public async Task<IActionResult> Logout()
        {
            APIResponse<string> response;
            // Retrieve the refresh token from the cookies
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return BadRequest("No refresh token found in cookies.");
            }
            try
            {
                // Invalidate the refresh token in the database
                await _userService.RevokeTokenAsync(refreshToken);

                // Remove the cookie
                Response.Cookies.Append("refreshToken", "", new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(-1), // Set expiration to the past
                    Secure = true, // Ensure it's HTTPS only if needed
                    SameSite = SameSiteMode.Strict // You can set SameSite as per your requirements
                });
                response = new() { Success = true };
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

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            APIResponse<string> response;
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshTokenJson))
            {
                return BadRequest("No refresh token found in cookies.");
            }
            try
            {
                var refrehToken = JsonSerializer.Deserialize<RefreshTokenDTO>(refreshTokenJson);
                string result = await _userService.TokenRefresher(refrehToken!.Token);

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
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDTO user)
        {
            APIResponse<UserDTO> response;
            var username = HttpContext.User.Identity?.Name;
            try
            {
                await _userUpdateValidator.ValidateAndThrowAsync(user);
                UserLoginResponse result = await _userService.UpdateUserInfoAsync(user, username!);

                if (!string.IsNullOrEmpty(result.JWToken)) // if we have a new JWT, we append a new cookie.
                {
                    CookieOptions jwtCookieOptions = new()
                    {
                        HttpOnly = true, // Prevents access from JavaScript
                        Expires = DateTime.UtcNow.AddMinutes(30) // Set expiry for refresh token
                    };
                    if (_webHostEnvironment.IsDevelopment())
                    {
                        jwtCookieOptions.SameSite = SameSiteMode.None;
                        jwtCookieOptions.Secure = false;
                    }
                    else
                    {
                        jwtCookieOptions.SameSite = SameSiteMode.Strict; // Prevents CSRF attacks
                        jwtCookieOptions.Secure = true; // Use HTTPS
                    }
                    Response.Cookies.Append("jwt", result.JWToken, jwtCookieOptions);
                }

                response = new()
                { Success = true, Result = result.User };
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
                {
                    errors.Add($"{error.PropertyName + ":"} {error.ErrorMessage}");
                }
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
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("Delete/{id:int}")]
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
}
