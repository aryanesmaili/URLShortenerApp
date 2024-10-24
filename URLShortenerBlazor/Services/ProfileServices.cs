using SharedDataModels.CustomClasses;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using System.Text.Json;
using URLShortenerBlazor.Services.Interfaces;

namespace URLShortenerBlazor.Services
{
    internal class ProfileServices : IProfileServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ProfileServices(IHttpClientFactory httpClientFactory)
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

        public async Task<APIResponse<PagedResult<URLDTO>>> GetProfileURLList(int userID, int pageNumber, int pageSize)
        {
            HttpRequestMessage req = new(HttpMethod.Get, $"api/Users/Profile/{userID}?pageNumber={pageNumber}&pageSize={pageSize}");

            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<PagedResult<URLDTO>>? result = await JsonSerializer.DeserializeAsync<APIResponse<PagedResult<URLDTO>>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return result!;
        }
    }
}
