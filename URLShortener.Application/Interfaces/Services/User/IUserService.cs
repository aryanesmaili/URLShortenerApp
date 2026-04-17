using SharedDataModels.Responses;
using URLShortener.Application.DTOs.EntityDTOs.User;

namespace URLShortener.Application.Interfaces.Services.User;

public interface IUserService
{
    Task<UserDTO> GetUserByIDAsync(long userId);
    Task<UserLoginResponse> UpdateUserInfoAsync(UserUpdateDTO newUserInfo, string requestingUsername);
    Task DeleteUserAsync(int userId);
    Task<UserDTO> SetNewEmailAsync(string newEmail, int userID, string reqUsername);
}
