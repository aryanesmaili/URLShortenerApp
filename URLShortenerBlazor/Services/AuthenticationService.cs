using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using System.Net.Http.Json;
using System.Security.Claims;
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
        private readonly HttpClient _authedHttpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly CustomAuthProvider _stateProvider;

        public AuthenticationService(ILocalStorageService localStorage, IHttpClientFactory httpClientFactory, HttpClient httpClient, CustomAuthProvider stateProvider)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClient;
            _authedHttpClient = _httpClientFactory.CreateClient("Auth");
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Make property name matching case-insensitive
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Handle camelCase JSON property names
            };
            _stateProvider = stateProvider;
        }

        public async Task<APIResponse<UserDTO>> Login(UserLoginDTO loginInfo)
        {

            APIResponse<UserDTO> result = await VerifyUser(loginInfo);

            if (result.Success) // if the login was succesful
            {
                await FetchCSRFTokens(); // we fetch the csrf token.

                await FetchUserRoles(); // we fetch the user roles for auth state.
            }
            return result!; // we return the response to show errors if any.
        }

        private async Task<APIResponse<UserDTO>> VerifyUser(UserLoginDTO loginInfo)
        {
            HttpRequestMessage req = new(HttpMethod.Post, "/api/Users/Login")
            {
                Content = new StringContent(JsonSerializer.Serialize(loginInfo), Encoding.UTF8, "application/json"),
            };
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            // we send user's login info to server to see if it's correct.
            HttpResponseMessage response = await _httpClient.SendAsync(req);
            var s = await response.Content.ReadAsStringAsync();
            // then we desrialize the server's response.
            APIResponse<UserDTO>? result = await JsonSerializer.DeserializeAsync<APIResponse<UserDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            if (response.IsSuccessStatusCode)
                await _localStorage.SetItemAsync("user", result!.Result); // we store the user data to local storage.

            return result!;
        }

        private async Task FetchCSRFTokens()
        {
            HttpRequestMessage req = new(HttpMethod.Get, "api/Users/antiforgery/token");
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage tokenresponse = await _httpClient.SendAsync(req);

            APIResponse<string>? res = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await tokenresponse.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
            if (tokenresponse.IsSuccessStatusCode)
            {
                await _localStorage.SetItemAsStringAsync("xsrf-token", res!.Result!);
            }
            else
            {
                throw new Exception($"Failed Fetching CSRF Token : {res!.ErrorMessage}");
            }
        }

        private async Task FetchUserRoles()
        {
            HttpRequestMessage req = new(HttpMethod.Get, "api/Users/GetRoles");// now we get the user's roles for authorization in blazor wasm.
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage res = await _authedHttpClient.SendAsync(req); // we get the roles item
            string s = await res.Content.ReadAsStringAsync();
            if (res.IsSuccessStatusCode)
            {
                // deserialize the role info.
                APIResponse<ClaimValue>? rawclaims = await JsonSerializer.DeserializeAsync<APIResponse<ClaimValue>>(await res.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
                // Create claims identity based on fetched claims
                List<Claim> claims =
                        [
                            new Claim(ClaimTypes.Email, rawclaims!.Result!.Email),
                                new Claim(ClaimTypes.Name, rawclaims.Result.Username),
                                new Claim(ClaimTypes.Role, rawclaims.Result.Role)
                        ];
                ClaimsPrincipal claim = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
                _stateProvider.MarkUserAsAuthenticated(claim); // we update the auth state.
                return;
            }
            throw new Exception("Error fetching User Roles.");
        }

        public async Task UpdateUserInfo(UserDTO user)
        {
            if (user == null)
                return;
            await _localStorage.SetItemAsync("user", user);
        }

        public async Task LogOutAsync()
        {
            var client = _httpClientFactory.CreateClient("Auth");

            await client.PostAsync("/api/Users/Logout", null); // Send null as there’s no content

            await _localStorage.RemoveItemAsync("xsrf-token"); // Remove local token 
            await _localStorage.RemoveItemAsync("user");
            _stateProvider.MarkUserAsLoggedOut();
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

        public async Task RefreshTokenAsync()
        {
            // Make a request to your refresh token endpoint
            HttpRequestMessage req = new(HttpMethod.Post, "api/Users/RefreshToken");
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<string>? result = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
            if (!response.IsSuccessStatusCode)
                throw new Exception(result!.ErrorMessage);
        }

    }
}
