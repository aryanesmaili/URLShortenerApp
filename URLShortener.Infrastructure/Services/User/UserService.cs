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
using URLShortener.Domain.Entities.URL;
using URLShortener.Domain.Entities.URLCategory;
using URLShortener.Domain.Entities.User;
using URLShortener.Domain.Enums;

namespace URLShortener.Infrastructure.Services.User;

public sealed class UserService(
                           IAuthService authorizationService,
                           IMapper mapper,
                           IEmailService emailService,
                           ICacheService cacheService,
                           HttpClient httpClient,
                           IUserRepository userRepository,
                           IURLRepository urlRepository,
                           IClickInfoRepository clickRepository,
                           IRefreshTokenRepository refreshTokenRepository,
                           IUnitOfWork uow) : IUserService
{
    private readonly IAuthService _authService = authorizationService;
    private readonly IMapper _mapper = mapper;
    private readonly IEmailService _emailService = emailService;
    private readonly ICacheService _cacheService = cacheService;
    private readonly HttpClient _httpClient = httpClient;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IURLRepository _urlRepository = urlRepository;
    private readonly IClickInfoRepository _clickRepository = clickRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IUnitOfWork _uow = uow;

    /// <summary>
    /// gets a user's info by their ID asynchronously.
    /// </summary>
    /// <param name="id"></param>
    /// <returns> an object containing showable user info</returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<UserDTO> GetUserByIDAsync(long id)
    {
        UserModel? user = await _userRepository.GetAsync(x => x.ID == id, asNoTracking: true)
            ?? throw new NotFoundException(nameof(UserModel), nameof(UserModel.ID), id);

        return _mapper.Map<UserDTO>(user);
    }

    /// <summary>
    /// Sets the new email for the user.
    /// </summary>
    /// <param name="newEmail">The new Email to be set</param>
    /// <param name="userID"></param>
    /// <param name="reqUsername"></param>
    /// <returns>a <see cref="UserDTO"/> object containing the new info to be set.</returns>
    public async Task<UserDTO> SetNewEmailAsync(string newEmail, int userID, string reqUsername)
    {
        UserModel user = await _authService.AuthorizeUserAccessAsync(userID, reqUsername);

        user.Email = newEmail;
        _userRepository.Update(user);
        await _uow.SaveChangesAsync();
        return _mapper.Map<UserDTO>(user);
    }

    /// <summary>
    /// updates a user's cred in database.
    /// </summary>
    /// <param name="newUserInfo">new information and changes.</param>
    /// <param name="requestingUsername">the username requesting the change.</param>
    /// <returns>a <see cref="UserDTO"/> object containing new record's info.</returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<UserLoginResponse> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, string requestingUsername)
    {
        UserModel user = await _authService.AuthorizeUserAccessAsync(newUserInfo.ID, requestingUsername);
        UserModel temp = new()
        {
            ID = user.ID,
            Name = user.Name,
            Email = user.Email,
            Username = user.Username,
            PasswordHash = user.PasswordHash,
            PasswordResetCode = user.PasswordResetCode,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            URLs = user.URLs != null ? new List<URLModel>(user.URLs) : null,
            URLCategories = user.URLCategories != null ? new List<URLCategoryModel>(user.URLCategories) : null,
            RefreshTokens = user.RefreshTokens != null ? new List<RefreshToken>(user.RefreshTokens) : null,
            FinancialRecord = user.FinancialRecord
        };

        user = _mapper.Map(newUserInfo, user);
        _userRepository.Update(user);
        await _uow.SaveChangesAsync();
        string jwToken = string.Empty;
        UserDTO userDTO = _mapper.Map<UserDTO>(user);
        // if the user's username has changed, we generate them a new JWT since we authorize via username.
        if (temp.Username != user.Username)
            jwToken = _authService.GenerateJWToken(user.Username, user.Role.ToString(), user.Email);

        UserLoginResponse response = new() { JWToken = jwToken, User = userDTO, RefreshToken = new() { Token = "" } };

        return response;
    }

    /// <summary>
    /// Deletes a user account from database.
    /// </summary>
    /// <param name="id">ID of the user to be deleted.</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task DeleteUserAsync(int id)
    {
        UserModel user = await _userRepository.GetAsync(x => x.ID == id)
            ?? throw new NotFoundException($"User {id} Does not Exist");
        _userRepository.Remove(user);
        await _uow.SaveChangesAsync();
    }
}
