using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using URLShortenerBlazor.Services.Interfaces;

namespace URLShortenerBlazor.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AuthenticationService(ILocalStorageService localStorage, IHttpClientFactory httpClientFactory, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClient;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Make property name matching case-insensitive
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Handle camelCase JSON property names
            };
        }

        public async Task<bool> IsLoggedInAsync()
        {
            var authToken = await _localStorage.GetItemAsync<string>("authToken");
            return !string.IsNullOrEmpty(authToken);
        }

        public async Task<APIResponse<UserDTO>> Login(UserLoginDTO loginInfo)
        {
            HttpRequestMessage req = new(HttpMethod.Post, "/api/Users/Login")
            {
                Content = new StringContent(JsonSerializer.Serialize(loginInfo), Encoding.UTF8, "application/json")
            };
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<UserDTO>? result = await JsonSerializer.DeserializeAsync<APIResponse<UserDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
            if (result!.Success)
            {
                await _localStorage.SetItemAsync("authToken", result!.Result!.JWToken!);
                await _localStorage.SetItemAsync("user", result.Result);
            }
            return result;
        }

        public async Task UpdateUserInfo(UserDTO user)
        {
            if (user == null)
                return;
            await _localStorage.SetItemAsync("authToken", user.JWToken); // Reassign the new token. 
            await _localStorage.SetItemAsync("user", user);
        }

        public async Task LogOutAsync()
        {
            var client = _httpClientFactory.CreateClient("Auth");

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

        public async Task<int> GetUserIDAsync()
        {
            return (await _localStorage.GetItemAsync<UserDTO>("user"))!.ID;
        }

        public async Task<UserDTO?> GetUserInfoAsync()
        {
            return await _localStorage.GetItemAsync<UserDTO>("user");
        }

        public async Task<string?> RefreshTokenAsync()
        {
            // Make a request to your refresh token endpoint
            HttpRequestMessage req = new(HttpMethod.Post, "api/Users/RefreshToken");
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage response = await _httpClient.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
                APIResponse<string>? result = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
                if (result != null && !string.IsNullOrEmpty(result.Result))
                {
                    // Store the new JWT token in local storage
                    await _localStorage.SetItemAsync("authToken", result.Result);
                    return result.Result;
                }
            }

            return null; // Return null if refreshing failed
        }
    }
}
