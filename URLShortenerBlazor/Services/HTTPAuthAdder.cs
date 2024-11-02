using Blazored.LocalStorage;
using SharedDataModels.Responses;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using URLShortenerBlazor.Services.Interfaces;

namespace URLShortenerBlazor.Services
{
    internal class HTTPAuthAdder : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;
        private readonly IAuthenticationService _authenticationService;

        public HTTPAuthAdder(ILocalStorageService localStorage, HttpClient httpClient, IAuthenticationService authenticationService)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
            _authenticationService = authenticationService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Add JWT token to the request
            var token = await _localStorage.GetItemAsync<string>("authToken", CancellationToken.None);
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); // setting the token in the header.
            }

            // Send the request
            var response = await base.SendAsync(request, cancellationToken);

            // Check if the response is Unauthorized (401) which means the jwt is expired.
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Attempt to refresh the token
                var refreshToken = await _authenticationService.RefreshTokenAsync();

                if (refreshToken != null)
                {
                    // Set the new JWT token in the request
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshToken);

                    // Retry the original request
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        private async Task<string?> RefreshTokenAsync()
        {
            // Make a request to your refresh token endpoint
            var response = await _httpClient.PostAsync("api/Users/RefreshToken", null); // Adjust endpoint as necessary

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<APIResponse<string>>(); // Assume TokenResponse has a property for JWT
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
