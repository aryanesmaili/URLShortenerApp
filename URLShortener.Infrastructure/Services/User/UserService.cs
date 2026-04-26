using AutoMapper;
using Microsoft.AspNetCore.Identity;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.Interfaces.Infrastructure.External;
using URLShortener.Application.Interfaces.Services.User;
using URLShortener.Application.Models;
using URLShortener.Application.Repositories;
using URLShortener.Application.Utility.Exceptions;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Infrastructure.Services.User;

public sealed class UserService(
                           IMapper mapper,
                           IEmailService emailService,
                           ICacheService cacheService,
                           HttpClient httpClient,
                           IUserRepository userRepository,
                           IURLRepository urlRepository,
                           IClickInfoRepository clickRepository,
                           IRefreshTokenRepository refreshTokenRepository,
                           IUnitOfWork uow,
                           UserManager<AppIdentityUser> userManager,
                           IAuthenticationService authService) : IUserService
{
    private readonly IMapper _mapper = mapper;
    private readonly IEmailService _emailService = emailService;
    private readonly ICacheService _cacheService = cacheService;
    private readonly HttpClient _httpClient = httpClient;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IURLRepository _urlRepository = urlRepository;
    private readonly IClickInfoRepository _clickRepository = clickRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IUnitOfWork _uow = uow;
    private readonly UserManager<AppIdentityUser> _userManager = userManager;
    private readonly IAuthenticationService _authService = authService;

    /// <summary>
    /// gets a user's info by their ID asynchronously.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns> an object containing showable user info</returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<UserDTO> GetUserByIDAsync(long userId)
    {
        UserModel? user = await _userRepository.GetAsync(x => x.ID == userId, asNoTracking: true)
            ?? throw new NotFoundException(nameof(UserModel), nameof(UserModel.ID), userId);

        return _mapper.Map<UserDTO>(user);
    }

    /// <summary>
    /// Sets a new email address for the user and updates both the domain and identity user records.
    /// </summary>
    /// <param name="newEmail">The new email address to set.</param>
    /// <param name="userID">The ID of the user whose email is being updated.</param>
    /// <returns>A <see cref="UserDTO"/> representing the updated user.</returns>
    /// <exception cref="NotFoundException">Thrown if the user or identity user is not found.</exception>
    /// <exception cref="Exception">Thrown if an error occurs during the update process.</exception>
    public async Task<UserDTO> SetNewEmailAsync(string newEmail, long userID)
    {
        try
        {
            using var _ = await _uow.BeginTransactionAsync();
            UserModel user = await _userRepository.GetAsync(x => x.ID == userID)
                ?? throw new NotFoundException(nameof(UserModel), nameof(UserModel.ID), userID);
            AppIdentityUser userIdentity = await _userManager.FindByIdAsync(user.ID.ToString())
                ?? throw new NotFoundException(nameof(AppIdentityUser), nameof(AppIdentityUser.Id), user.ID);
            user.Email = newEmail;
            userIdentity.Email = newEmail;
            _userRepository.Update(user);
            await _userManager.UpdateAsync(userIdentity);
            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();
            return _mapper.Map<UserDTO>(user);
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync();
            // log err Here
            throw;
        }
    }

    /// <summary>
    /// Updates user information such as username and name. If either is changed, updates both the domain and identity user records in a transaction.
    /// </summary>
    /// <param name="newUserInfo">The new user information to update.</param>
    /// <param name="userId">The ID of the user to update.</param>
    /// <returns>A <see cref="UserDTO"/> representing the updated user.</returns>
    /// <exception cref="NotFoundException">Thrown if the user or identity user is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown if updating the identity user fails.</exception>
    /// <exception cref="Exception">Thrown if an error occurs during the update process.</exception>
    public async Task<UserDTO> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, long userId)
    {
        UserModel user = await _userRepository.GetAsync(x => x.ID == userId)
            ?? throw new NotFoundException(nameof(UserModel), nameof(UserModel.ID), userId);
        // run checks
        bool usernameChanged = IsUsernameBeingChanged(newUserInfo, user);
        bool nameChanged = IsNameBeingChanged(newUserInfo, user);
        if (usernameChanged || nameChanged)
        {
            try
            {
                using var _ = await _uow.BeginTransactionAsync();
                AppIdentityUser userIdentity = await _userManager.FindByIdAsync(user.ID.ToString())
                    ?? throw new NotFoundException(nameof(AppIdentityUser), nameof(AppIdentityUser.Id), user.ID);
                userIdentity.UserName = newUserInfo.Username;
                userIdentity.DisplayName = newUserInfo.Name;
                user = _mapper.Map(newUserInfo, user);
                _userRepository.Update(user);
                IdentityResult result = await _userManager.UpdateAsync(userIdentity);
                if (!result.Succeeded)
                    throw new InvalidOperationException("Failed to update user identity");
                await _uow.SaveChangesAsync();
                await _uow.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _uow.RollbackTransactionAsync();
                // log error
                throw;
            }
        }
        else
        {
            user = _mapper.Map(newUserInfo, user);
            _userRepository.Update(user);
            await _uow.SaveChangesAsync();
        }
        UserDTO userDTO = _mapper.Map<UserDTO>(user);
        return userDTO;
    }

    /// <summary>
    /// Determines if the username is being changed.
    /// </summary>
    /// <param name="newUser">The new user information.</param>
    /// <param name="oldUser">The existing user information.</param>
    /// <returns>True if the username is being changed; otherwise, false.</returns>
    private static bool IsUsernameBeingChanged(UserUpdateDTO newUser, UserModel oldUser)
        => newUser.Username != null && newUser.Username != oldUser.Username;

    /// <summary>
    /// Determines if the name is being changed.
    /// </summary>
    /// <param name="newUser">The new user information.</param>
    /// <param name="oldUser">The existing user information.</param>
    /// <returns>True if the name is being changed; otherwise, false.</returns>
    private static bool IsNameBeingChanged(UserUpdateDTO newUser, UserModel oldUser)
        => newUser.Name != null && newUser.Name != oldUser.Name;

    /// <summary>
    /// Deletes a user and performs a soft delete on the associated identity user and Domain User.
    /// The operation is executed within a transaction to ensure data consistency.
    /// </summary>
    /// <param name="userId">The ID of the user to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown if the user or the associated identity user does not exist.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown if an error occurs during the deletion process. The transaction is rolled back in this case.
    /// </exception>
    public async Task DeleteUserAsync(long userId)
    {
        try
        {
            using var _ = await _uow.BeginTransactionAsync();
            UserModel user = await _userRepository.GetAsync(x => x.ID == userId)
                ?? throw new NotFoundException(nameof(UserModel), nameof(UserModel.ID), userId);
            AppIdentityUser userIdentity = await _userManager.FindByIdAsync(user.ID.ToString())
                ?? throw new NotFoundException(nameof(AppIdentityUser), nameof(AppIdentityUser.Id), user.ID);
            _userRepository.Remove(user);
            _authService.SoftDeleteIdentityUser(userIdentity);
            await _uow.SaveChangesAsync();
            await _userManager.UpdateAsync(userIdentity);
            await _uow.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync();
            // log error here
            throw;
        }
    }
}
