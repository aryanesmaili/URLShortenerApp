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


        public async Task<APIResponse<UserDashboardDTO>> GetDashboardInfo(int userID)
        {
            HttpRequestMessage req = new(HttpMethod.Get, $"api/Users/Dashboard/{userID}");

            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<UserDashboardDTO>? result = await JsonSerializer.DeserializeAsync<APIResponse<UserDashboardDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return result!;
        }
    }
}
