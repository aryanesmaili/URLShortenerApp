using Blazored.LocalStorage;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using System.Net.Http.Json;
using URLShortenerBlazor.Services.Interfaces;

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

        public async Task<APIResponse<UserDTO>> Login(UserLoginDTO loginInfo)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/Users/Login", loginInfo);

            APIResponse<UserDTO>? result = await response.Content.ReadFromJsonAsync<APIResponse<UserDTO>>();
            if (result!.Success)
            {
                await _localStorage.SetItemAsync("authToken", result!.Result!.JWToken!);
                await _localStorage.SetItemAsync("user", result.Result);
            }
            return result;
        }

        public async Task LogOut()
        {
            var client = _httpClientFactory.CreateClient("Auth");
            // Call the Logout endpoint: no content is needed as it will read the refreshToken from the cookies
            await client.PostAsync("/api/Users/Logout", null); // Send null as there’s no content

            await _localStorage.RemoveItemAsync("authToken"); // Remove local token 
            await _localStorage.RemoveItemAsync("user");
        }

        public async Task<APIResponse<UserDTO>> Register(UserCreateDTO userCreateDTO)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/Users/Register", userCreateDTO);

            APIResponse<UserDTO>? result = await response.Content.ReadFromJsonAsync<APIResponse<UserDTO>>();

            return result!;
        }

        public async Task<int> GetUserID()
        {
            return (await _localStorage.GetItemAsync<UserDTO>("user"))!.ID;
        }
    }
}
