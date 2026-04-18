using AutoMapper;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Application.Interfaces.Infrastructure.External;
using URLShortener.Application.Interfaces.Services.User;
using URLShortener.Application.Repositories;
using URLShortener.Application.Utility.Exceptions;
using URLShortener.Domain.Entities.User;

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

    public async Task<UserDTO> SetNewEmailAsync(string newEmail, long userID)
    {
        UserModel user = await _userRepository.GetAsync(x => x.ID == userID)
            ?? throw new NotFoundException(nameof(UserModel), nameof(UserModel.ID), userID);
        user.Email = newEmail;
        _userRepository.Update(user);
        await _uow.SaveChangesAsync();
        return _mapper.Map<UserDTO>(user);
    }

    public async Task<UserDTO> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, long userId)
    {
        UserModel user = await _userRepository.GetAsync(x => x.ID == userId)
            ?? throw new NotFoundException(nameof(UserModel), nameof(UserModel.ID), userId);
        user = _mapper.Map(newUserInfo, user);
        _userRepository.Update(user);
        await _uow.SaveChangesAsync();
        UserDTO userDTO = _mapper.Map<UserDTO>(user);
        return userDTO;
    }

    /// <summary>
    /// Deletes a user account from database.
    /// </summary>
    /// <param name="id">ID of the user to be deleted.</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task DeleteUserAsync(long id)
    {
        UserModel user = await _userRepository.GetAsync(x => x.ID == id)
            ?? throw new NotFoundException($"User {id} Does not Exist");
        _userRepository.Remove(user);
        await _uow.SaveChangesAsync();
    }
}
