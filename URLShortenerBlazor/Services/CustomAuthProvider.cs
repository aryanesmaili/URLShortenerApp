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

        /// <summary>
        /// Returns the authentication state of the user. first it checks the local variable for auth info
        /// if there's none, it means that the user is either logged out or this is a new seesion.
        /// if the user is logged in, but it's a new seesion, it will request user's roles from backend because there's a csrf token in local storage.
        /// if the user is not logged in and it's a new session, it will return UnAuthenticated.
        /// </summary>
        /// <returns></returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_cachedUser == null && await _localStorage.ContainKeyAsync("xsrf-token")) // it's a new seesion. the user has token but no auth data.
            {
                try
                {
                    await FetchCSRFTokens(); // get the httponly cookie of csrf 

                    // If the user object is null and does not contain claims, we try to fill it from backend.
                    _cachedUser = await FetchUserRoles();
                }
                catch (Exception e)
                {
                    // if anything goes wrong
                    Console.WriteLine(e.Message);
                    _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
                }
            }
            else
            {
                // the user is not logged in and this is a new seesion, so we return empty.
                _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
            }

            return new AuthenticationState(_cachedUser);
        }

        /// <summary>
        /// changes the user's authentication status to Authenticated. by setting the local auth info object to the given not nulled object.
        /// </summary>
        /// <param name="user"></param>
        public void MarkUserAsAuthenticated(ClaimsPrincipal user)
        {
            ArgumentNullException.ThrowIfNull(user);
            _cachedUser = user;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        /// <summary>
        /// Changes the user's authentication status to Not Authenticated.
        /// </summary>
        public void MarkUserAsLoggedOut()
        {
            _cachedUser = null;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }

        /// <summary>
        /// checks if a user has the given role.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool IsUserInRole(string role)
        {
            return _cachedUser?.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.Equals(role, StringComparison.CurrentCultureIgnoreCase)) ?? false;
        }

        /// <summary>
        /// Retireves user's Authentication data from backend to form a Authentication status.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<ClaimsPrincipal> FetchUserRoles()
        {
            HttpRequestMessage req = new(HttpMethod.Get, "api/Users/GetRoles");// now we get the user's roles for authorization in blazor wasm.
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage res = await _httpClient.SendAsync(req); // we get the roles item
            try
            {
                ClaimsPrincipal claim = new(new ClaimsIdentity());
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
                    return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
                }
                return new ClaimsPrincipal(new ClaimsIdentity());
            }
            catch (Exception e)
            {
                throw new Exception($"Error fetching User Roles : {e.Message}");
            }
        }

        /// <summary>
        /// Fetches the CSRF tokens required for protecting against CSRF attacks. writes one of the tokens to localstorage.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown if the fetching goes wrong.</exception>
        public async Task FetchCSRFTokens()
        {
            HttpRequestMessage req = new(HttpMethod.Get, "api/Users/antiforgery/token");
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage tokenresponse = await _httpClient.SendAsync(req);

            APIResponse<string>? res = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await tokenresponse.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            if (tokenresponse.IsSuccessStatusCode)
                await _localStorage.SetItemAsStringAsync("xsrf-token", res!.Result!); // the token that will be included in the header of the requests.
            else
                throw new Exception($"Failed Fetching CSRF Token : {res!.ErrorMessage}");
        }
    }
}
