using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using SharedDataModels.Responses;
using System.Text.Json;

namespace URLShortenerBlazor.Services
{
    internal class HTTPAuthAdder : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public HTTPAuthAdder(ILocalStorageService localStorage, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Make property name matching case-insensitive
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Handle camelCase JSON property names
            };
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            // Read CSRF token to add to the request
            string? token = await _localStorage.GetItemAsync<string>("xsrf-token", CancellationToken.None);

            // add the token to header of the request.
            if (!string.IsNullOrEmpty(token))
                request.Headers.Add("X-XSRF-TOKEN", token); // setting the token in the header.
            else // if we don't have the token, we get it from backend.
                await GetCSRFTokenAsync();

            // Send the request
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // Check if the response is Unauthorized (401) which means the jwt is expired.
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Attempt to refresh the token
                await RefreshTokenAsync();

                // Retry the original request
                response = await base.SendAsync(request, cancellationToken);
            }
            return response;
        }

        /// <summary>
        /// Sends a Request to Backend to refresh the JWT Token using the refresh token.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task RefreshTokenAsync()
        {
            // Send a request to your refresh token endpoint
            HttpRequestMessage req = new(HttpMethod.Post, "api/Users/RefreshToken");
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include); // include the cookies.
            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<string>? result = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            if (!response.IsSuccessStatusCode)
                throw new Exception(result!.ErrorMessage);
        }

        /// <summary>
        /// Gets the CSRF Tokens from backend to prevent CSRF Attacks.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task GetCSRFTokenAsync()
        {
            HttpRequestMessage req = new(HttpMethod.Get, "api/Users/antiforgery/token");
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage tokenresponse = await _httpClient.SendAsync(req);

            APIResponse<string>? res = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await tokenresponse.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            if (tokenresponse.IsSuccessStatusCode)
                await _localStorage.SetItemAsStringAsync("xsrf-token", res!.Result!);

            else
                throw new Exception($"Failed Fetching CSRF Token : {res!.ErrorMessage}");
        }
    }
}
