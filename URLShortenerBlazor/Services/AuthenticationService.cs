using Blazored.LocalStorage;
using SharedDataModels.DTOs;
using System.Net.Http.Json;
using System.Net;
using URLShortenerBlazor.Services.Interfaces;
using System.Text.Json;
namespace URLShortenerBlazor.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;

        public AuthenticationService(ILocalStorageService localStorage, IHttpClientFactory httpClientFactory, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClient;
        }

        public async Task<bool> IsLoggedInAsync()
        {
            var authToken = await _localStorage.GetItemAsync<string>("authToken");
            return !string.IsNullOrEmpty(authToken);
        }

        public async Task Login(UserLoginDTO loginInfo)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/Users/Login", loginInfo);

            if (response.IsSuccessStatusCode)
            {
                UserDTO? result = await response.Content.ReadFromJsonAsync<UserDTO>();
                await _localStorage.SetItemAsync("authToken", result!.JWToken!);
                await _localStorage.SetItemAsync("user", result);
                return;
            }
            throw new Exception(await response.Content.ReadAsStringAsync());
        }

        public async Task LogOut()
        {
            var client = _httpClientFactory.CreateClient("Auth");
            // Call the Logout endpoint: no content is needed as it will read the refreshToken from the cookies
            await client.PostAsync("/api/Users/Logout", null); // Send null as there’s no content

            await _localStorage.RemoveItemAsync("authToken"); // Remove local token 

        }

        public async Task Register(UserCreateDTO userCreateDTO)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/Users/Register", userCreateDTO);

            if (response.IsSuccessStatusCode)
                return;

            throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
}
