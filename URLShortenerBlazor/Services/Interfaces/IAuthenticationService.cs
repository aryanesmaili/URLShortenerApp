using SharedDataModels.DTOs;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<bool> IsLoggedInAsync();
        public Task LogOut();
        public Task Login(UserLoginDTO userLogin);
        public Task Register(UserCreateDTO userCreateDTO);
    }
}