using URLShortener.Application.DTOs.EntityDTOs.User;

namespace URLShortener.Application.Interfaces.Services.User;

public interface IUserService
{
    Task<UserDTO> GetUserByIDAsync(long userId);
    Task DeleteUserAsync(long userId);
    Task<UserDTO> SetNewEmailAsync(string newEmail, long userID);
    Task<UserDTO> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, long userId);
}
