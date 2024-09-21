using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pexita.Utility.Exceptions;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using URLShortenerAPI.Data.Entites.User;
using URLShortenerAPI.Services.Interfaces;
using YamlDotNet.Core.Tokens;

namespace URLShortenerAPI.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    internal class UsersController : ControllerBase
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
                var result = await _userService.GetFullUserInfo(id);
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
        [HttpGet("{username:string}")]
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

                var result = await _userService.LoginUserAsync(LoginInfo);
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
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] UserCreateDTO userCreateDTO)
        {
            try
            {
                await _userValidator.ValidateAndThrowAsync(userCreateDTO);

                var result = await _userService.RegsiterUserAsync(userCreateDTO);
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
        public async Task<IActionResult> CheckResetCode([FromQuery] string code, [FromBody] UserDTO user)
        {
            try
            {
                var result = await _userService.CheckResetCode(user, code);
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
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest reqInfo)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            try
            {
                var result = await _userService.ChangePasswordAsync(reqInfo, username!);
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
        public async Task<IActionResult> Logout(string token)
        {
            try
            {
                await _userService.RevokeToken(token);
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
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            try
            {
                await _userService.RevokeToken(refreshToken);
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
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromForm] UserUpdateDTO user)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            try
            {
                await _userService.UpdateUserInfoAsync(user, username!);
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
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            try
            {
                await _userService.DeleteUser(id);
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
