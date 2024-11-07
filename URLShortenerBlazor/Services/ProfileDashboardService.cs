using Microsoft.AspNetCore.Components.WebAssembly.Http;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using System.Text.Json;
using URLShortenerBlazor.Services.Interfaces;

namespace URLShortenerBlazor.Services
{
    internal class ProfileDashboardService : IProfileDashboardService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        public ProfileDashboardService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient("Auth");
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Make property name matching case-insensitive
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Handle camelCase JSON property names
            };
        }

        /// <summary>
        /// Gets Dashboard's Required Data from backend.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns>a <see cref="UserDashboardDTO"/> object containing the required info for dashboard charts and sections.</returns>
        public async Task<APIResponse<UserDashboardDTO>> GetDashboardInfo(int userID)
        {
            HttpRequestMessage req = new(HttpMethod.Get, $"api/Users/Dashboard/{userID}");
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<UserDashboardDTO>? result = await JsonSerializer.DeserializeAsync<APIResponse<UserDashboardDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return result!;
        }

        /// <summary>
        /// Gets the info required to be shown in profile stats.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns> a <see cref="UserStats"/> object containing 4 needed elements.</returns>
        public async Task<APIResponse<UserStats>> GetUserStats(int userID)
        {
            HttpRequestMessage req = new(HttpMethod.Get, $"api/Users/Profile/{userID}");
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<UserStats>? result = await JsonSerializer.DeserializeAsync<APIResponse<UserStats>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return result!;
        }

    }
}
