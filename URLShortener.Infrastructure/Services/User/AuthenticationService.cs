using AutoMapper;
using Microsoft.Extensions.Configuration;
using SharedDataModels.Responses;
using System.Text.Json;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.Interfaces.Infrastructure.External;
using URLShortener.Application.Interfaces.Services.User;
using URLShortener.Application.Repositories;
using URLShortener.Application.Utility.Exceptions;
using URLShortener.Common.HelperFunctions;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Infrastructure.Services.User;

public sealed class AuthenticationService(IMapper mapper,
    IUserRepository userRepository,
    IAuthService authService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork uow,
    IEmailService emailService,
    IConfiguration settings,
    HttpClient httpClient) : IAuthenticationService
{
    private readonly IMapper _mapper = mapper;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IAuthService _authService = authService;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IUnitOfWork _uow = uow;
    private readonly IEmailService _emailService = emailService;
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _secretKey = settings["TurnstileSecret"]!;

    public async Task<UserLoginResponse> LoginUserAsync(UserLoginDTO info)
    {
        UserModel? user = null;
        if (info.Identifier!.IsEmail())

            user = await _userRepository.GetAsync(x => x.Email == info.Identifier)
                ?? throw new NotFoundException($"Username or Password is wrong");
        else
            user = await _userRepository.GetAsync(u => u.Username == info.Identifier)
                ?? throw new NotFoundException($"Username or Password is wrong");

        if (user == null || !BCrypt.Net.BCrypt.Verify(info.Password, user?.PasswordHash))
            throw new ArgumentException("Username or Password is not correct");

        UserDTO userDTO = _mapper.Map<UserDTO>(user!);
        string jwtoken = _authService.GenerateJWToken(user!.Username, user.Role.ToString(), user.Email);
        string rawRefreshToken = _authService.GenerateRefreshToken();
        RefreshToken refreshToken = new()
        {
            Token = rawRefreshToken,
            User = user,
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            UserId = user.ID
        };

        _refreshTokenRepository.Add(refreshToken);
        await _uow.SaveChangesAsync();

        RefreshTokenDTO refreshTokenDTO = _mapper.Map<RefreshTokenDTO>(refreshToken);
        UserLoginResponse response = new()
        { User = userDTO, RefreshToken = refreshTokenDTO, JWToken = jwtoken };

        return response;
    }

    public async Task<UserDTO> RegisterUserAsync(UserCreateDTO newUserInfo)
    {
        UserModel newUser = _mapper.Map<UserModel>(newUserInfo);
        newUser.CreatedAt = DateTime.UtcNow;
        newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUserInfo.Password);
        newUser.FinancialRecord = new() { User = newUser, Balance = 0 };

        _userRepository.Add(newUser);
        await _uow.SaveChangesAsync();

        return _mapper.Map<UserDTO>(newUser);
    }

    public async Task<CaptchaVerificationResponse> VerifyCaptcha(string token, string userIP)
    {
        const string cloudflareURL = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

        FormUrlEncodedContent formData = new(
        [
        new KeyValuePair<string, string>("secret", _secretKey),
        new KeyValuePair<string, string>("response", token),
        new KeyValuePair<string, string>("remoteip", userIP ?? string.Empty)
    ]);

        HttpResponseMessage response = await _httpClient.PostAsync(cloudflareURL, formData);

        if (!response.IsSuccessStatusCode)
        {
            return new CaptchaVerificationResponse { Success = false, ErrorCodes = ["bad-request"] };
        }

        CaptchaVerificationResponse? captchaResponse = await JsonSerializer.DeserializeAsync<CaptchaVerificationResponse>(await response.Content.ReadAsStreamAsync());

        return captchaResponse ?? new CaptchaVerificationResponse { Success = false, ErrorCodes = ["public-error"] };
    }

    public async Task RevokeTokenAsync(string token)
    {
        if (token == null)
            throw new ArgumentNullException(token);

        RefreshToken tokenRecord = await _refreshTokenRepository.GetAsync(t => t.Token == token)
            ?? throw new NotFoundException("Refresh Token Not Found.");
        if (tokenRecord != null && tokenRecord.IsActive)
        {
            tokenRecord.Revoked = DateTime.UtcNow;
            _refreshTokenRepository.Update(tokenRecord);
            await _uow.SaveChangesAsync();
            return;
        }
        throw new Exception("token either invalid or already inactive.");
    }

    public async Task<string> TokenRefresher(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentNullException(refreshToken);

        RefreshToken? currentRefreshToken = await _refreshTokenRepository.GetAsync(t => t.Token == refreshToken);

        if (currentRefreshToken == null)
            throw new NotFoundException($"token {refreshToken} is not valid.");

        else if (!currentRefreshToken.IsActive)
            throw new RefreshTokenExpiredException("RefreshToken is Expired");

        UserModel user = await _userRepository.GetAsync(x => x.ID == currentRefreshToken.UserId)
            ?? throw new NotFoundException($"User {currentRefreshToken.UserId} Does not exist");

        string jwToken = _authService.GenerateJWToken(user.Username, user.Role.ToString(), user.Email);

        return jwToken;
    }

    public async Task ResetEmailAsync(int userID, string reqUsername)
    {
        UserModel user = await _authService.AuthorizeUserAccessAsync(userID, reqUsername);
        user.EmailResetCode = _authService.GenerateRandomPassword(8);
        string Subject = "Pexita Authentication code";
        string Body = $"Your Authentication Code Is {user.PasswordResetCode}";

        _userRepository.Update(user);
        await _uow.SaveChangesAsync();

        await _emailService.SendEmail(user.Email, Subject, Body);
    }

    public async Task CheckEmailResetCodeAsync(string code, int userID, string reqUsername)
    {
        UserModel user = await _authService.AuthorizeUserAccessAsync(userID, reqUsername);

        var resetCode = user.EmailResetCode;

        if (resetCode != code)
            throw new ArgumentException("Code is Wrong.");
        user.EmailResetCode = null;
        _userRepository.Update(user);
        await _uow.SaveChangesAsync();
    }

    public async Task ResetPasswordAsync(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            throw new ArgumentNullException("Invalid Input.");

        UserModel? user;

        if (identifier.IsEmail()) // if the user has entered an email:
            user = await _userRepository.GetAsync(u => u.Email == identifier); // we search by email

        else // if it's not an email then the user has entered their username
            user = await _userRepository.GetAsync(user => user.Username == identifier); // we search by username

        if (user == null) // if no user exists with that email/username:
            throw new NotFoundException($"User {identifier} does not exist.");

        user.PasswordResetCode = _authService.GenerateRandomPassword(8); // we generate a reset password code for them,
        string Subject = "Pexita Authentication code";
        string Body = $"Your Authentication Code Is {user.PasswordResetCode}";

        _userRepository.Update(user);
        await _uow.SaveChangesAsync();

        await _emailService.SendEmail(user.Email, Subject, Body); // we send the code to the user.
    }

    public async Task<UserLoginResponse> CheckPasswordResetCodeAsync(string identifier, string Code)
    {
        if (string.IsNullOrEmpty(Code))
            throw new ArgumentNullException(nameof(Code));

        UserModel? userRec;

        if (identifier.IsEmail()) // if the user has entered an email:
            userRec = await _userRepository.GetAsync(u => u.Email == identifier); // we search by email
        else // if it's not an email then the user has entered their username
            userRec = await _userRepository.GetAsync(user => user.Username == identifier); // we search by username

        string ResetCode = userRec!.PasswordResetCode ?? throw new ArgumentNullException("ResetCode");

        if (ResetCode != Code)
            throw new ArgumentException("Code is Wrong.");

        var result = _mapper.Map<UserDTO>(userRec);
        string token = _authService.GenerateJWToken(userRec.Username, userRec.Role.ToString(), userRec.Email);
        string refToken = _authService.GenerateRefreshToken();

        RefreshToken refreshToken = new()
        {
            Token = refToken,
            User = userRec,
            UserId = userRec.ID,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        _refreshTokenRepository.Add(refreshToken);
        await _uow.SaveChangesAsync();

        RefreshTokenDTO refreshTokenDTO = _mapper.Map<RefreshTokenDTO>(refreshToken);

        UserLoginResponse response = new()
        { User = result, RefreshToken = refreshTokenDTO, JWToken = token };
        return response;
    }

    public async Task<UserDTO> ChangePasswordAsync(ChangePasswordRequest reqInfo, string requestingUsername)
    {

        if (string.IsNullOrEmpty(reqInfo.NewPassword) || string.IsNullOrEmpty(reqInfo.ConfirmPassword))
            throw new ArgumentNullException(nameof(reqInfo.NewPassword));

        else if (reqInfo.NewPassword != reqInfo.ConfirmPassword)
            throw new ArgumentException($"Entered values {reqInfo.NewPassword} and {reqInfo.ConfirmPassword} Do not match.");

        UserModel user = await _authService.AuthorizeUserAccessAsync(reqInfo.UserInfo.ID, requestingUsername);

        string hashedpassword = BCrypt.Net.BCrypt.HashPassword(reqInfo.NewPassword);
        if (hashedpassword == user.PasswordHash)
            throw new ArgumentException("input password is no different from the current password.");

        user.PasswordHash = hashedpassword;
        user.PasswordResetCode = null;
        _userRepository.Update(user);
        await _uow.SaveChangesAsync();

        return _mapper.Map<UserDTO>(user);
    }

}
