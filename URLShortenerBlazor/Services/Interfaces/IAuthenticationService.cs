namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<bool> IsLoggedInAsync();
        public Task LogOut();
        public Task Login(string token);
    }
}