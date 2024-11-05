using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
namespace URLShortenerBlazor.Services
{
    public class CustomAuthProvider : AuthenticationStateProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private ClaimsPrincipal? _cachedUser;

        public CustomAuthProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient("Auth");
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_cachedUser != null)
            {
                // Return cached user if available
                return new AuthenticationState(_cachedUser);
            }

            // If the user is not authenticated or fetching claims failed
            _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
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
    }
}
