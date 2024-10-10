using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pexita.Utility.Exceptions;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using URLShortenerAPI.Data.Entities.User;
using URLShortenerAPI.Services.Interfaces;
using URLShortenerAPI.Utility.Exceptions;
using YamlDotNet.Core.Tokens;

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

        public UsersController(IUserService userService,
            IValidator<UserCreateDTO> userValidator,
            IValidator<UserLoginDTO> userLoginValidator,
            IValidator<UserUpdateDTO> userUpdateValidator)
        {
            _userService = userService;
            _userValidator = userValidator;
            _userLoginValidator = userLoginValidator;
            _userUpdateValidator = userUpdateValidator;
        }
        [Authorize(Policy = "AllUsers")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            try
            {
                var result = await _userService.GetUserByIDAsync(id);
                return Ok(result);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
        [Authorize(Policy = "AllUsers")]
        [HttpGet("Profile/{id:int}")]
        public async Task<IActionResult> GetUserFullInfo([FromRoute] int id)
        {
            try
            {
                var result = await _userService.GetFullUserInfoAsync(id);
                return Ok(result);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
        [Authorize(Policy = "AllUsers")]
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsername([FromRoute] string username)
        {
            try
            {
                var result = await _userService.GetUserByUsernameAsync(username);
                return Ok(result);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromForm] UserLoginDTO LoginInfo)
        {
            try
            {
                await _userLoginValidator.ValidateAndThrowAsync(LoginInfo);

                UserLoginResponse result = await _userService.LoginUserAsync(LoginInfo);
                CookieOptions? cookieOptions = new()
                {
                    HttpOnly = true, // Prevents access from JavaScript
                    Secure = true,   // Use HTTPS
                    SameSite = SameSiteMode.Strict, // Prevents CSRF attacks
                    Expires = DateTime.UtcNow.AddDays(7) // Set expiry for refresh token
                };
                Response.Cookies.Append("refreshToken", JsonSerializer.Serialize(result.RefreshToken), cookieOptions);
                return Ok(result.User);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                var errorResponse = new ErrorResponse
                {
                    Message = e.Message,
                    InnerException = e.InnerException?.ToString() ?? "",
                    StackTrace = e.StackTrace ?? ""
                };
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] UserCreateDTO userCreateDTO)
        {
            try
            {
                await _userValidator.ValidateAndThrowAsync(userCreateDTO);

                var result = await _userService.RegisterUserAsync(userCreateDTO);
                return Ok(result);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromForm] string identifier)
        {
            try
            {
                var result = await _userService.ResetPasswordAsync(identifier);
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

        [HttpPost("CheckResetCode")]
        public async Task<IActionResult> CheckResetCode([FromQuery] string code, [FromBody] string identifier)
        {
            try
            {
                UserLoginResponse result = await _userService.CheckResetCodeAsync(identifier, code);
                var cookieOptions = new CookieOptions()
                {
                    HttpOnly = true, // Prevents access from JavaScript
                    Secure = true,   // Use HTTPS
                    SameSite = SameSiteMode.Strict, // Prevents CSRF attacks
                    Expires = DateTime.UtcNow.AddDays(7) // Set expiry for refresh token
                };
                Response.Cookies.Append("refreshToken", JsonSerializer.Serialize(result.RefreshToken), cookieOptions);

                return Ok(result.User);
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
        [Authorize(Policy = "AllUsers")]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest reqInfo)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            try
            {
                var result = await _userService.ChangePasswordAsync(reqInfo, username!);

                Response.Cookies.Append("refreshToken", Request.Cookies["refreshToken"]!);
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

        [Authorize(Policy = "AllUsers")]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            try
            {
                await _userService.RevokeTokenAsync(refreshToken);
                Response.Cookies.Append("refreshToken", "", new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(-1), // Set expiration to the past
                    Secure = true, // Ensure it's HTTPS only if needed
                    SameSite = SameSiteMode.Strict // You can set SameSite as per your requirements
                });
                return Ok();
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
        [Authorize(Policy = "AllUsers")]
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var result = await _userService.TokenRefresher(refreshToken);

                Response.Cookies.Append("refreshToken", Request.Cookies["refreshToken"]!);
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
        [Authorize(Policy = "AllUsers")]
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromForm] UserUpdateDTO user)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            try
            {
                var result = await _userService.UpdateUserInfoAsync(user, username!);

                Response.Cookies.Append("refreshToken", Request.Cookies["refreshToken"]!);

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
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return Ok();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
