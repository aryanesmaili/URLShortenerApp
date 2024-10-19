using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Text;
using URLShortenerBlazor.Services.Interfaces;
using static System.Net.WebRequestMethods;
namespace URLShortenerBlazor.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthenticationService(ILocalStorageService localStorage, IHttpClientFactory httpClientFactory)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> IsLoggedInAsync()
        {
            var authToken = await _localStorage.GetItemAsync<string>("authToken");
            return !string.IsNullOrEmpty(authToken);
        }

        public async Task Login(string token)
        {
            await _localStorage.SetItemAsync("authToken", token);
        }

        public async Task LogOut()
        {
            var client = _httpClientFactory.CreateClient("Auth");
            // Call the Logout endpoint: no content is needed as it will read the refreshToken from the cookies
            await client.PostAsync("/api/Users/Logout", null); // Send null as there’s no content

            await _localStorage.RemoveItemAsync("authToken"); // Remove local token 

        }
    }
}
