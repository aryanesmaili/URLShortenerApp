using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using System.Security.Claims;
using System.Text.Json;
namespace URLShortenerBlazor.Services
{
    public class CustomAuthProvider : AuthenticationStateProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private ClaimsPrincipal? _cachedUser;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly ILocalStorageService _localStorage;
        public CustomAuthProvider(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient("Auth");
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Make property name matching case-insensitive
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Handle camelCase JSON property names
            };
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_cachedUser == null && await _localStorage.ContainKeyAsync("xsrf-token"))
            {
                try
                {
                    // If the user object is null and does not contain claims, we try to fill it from backend.
                    _cachedUser = await FetchUserRoles();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
                }
            }
            else
            {
                _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
            }

            return new AuthenticationState(_cachedUser);
        }

        public void MarkUserAsAuthenticated(ClaimsPrincipal user)
        {
            _cachedUser = user;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void MarkUserAsLoggedOut()
        {
            _cachedUser = null;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }

        public bool IsUserInRole(string role)
        {
            return _cachedUser?.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.ToLower() == role.ToLower()) ?? false;
        }

        private async Task<ClaimsPrincipal> FetchUserRoles()
        {
            HttpRequestMessage req = new(HttpMethod.Get, "api/Users/GetRoles");// now we get the user's roles for authorization in blazor wasm.
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage res = await _httpClient.SendAsync(req); // we get the roles item
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
                return claim;
            }
            throw new Exception("Error fetching User Roles.");
        }
    }
}
