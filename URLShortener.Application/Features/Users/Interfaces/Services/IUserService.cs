using URLShortener.Application.Features.Users.DTOs;

namespace URLShortener.Application.Features.Users.Interfaces.Services;

public interface IUserService
{
    Task<UserDTO> GetUserByIDAsync(long userId);
    Task DeleteUserAsync(long userId);
    Task<UserDTO> SetNewEmailAsync(string newEmail, long userID);
    Task<UserDTO> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, long userId);
}
