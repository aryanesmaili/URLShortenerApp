using SharedDataModels.CustomClasses;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using System.Text.Json;
using URLShortenerBlazor.Services.Interfaces;

namespace URLShortenerBlazor.Services
{
    internal class URLTableService : IURLTableService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public URLTableService(IHttpClientFactory httpClientFactory)
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
        /// Gets the paginated result for the table to show.
        /// </summary>
        /// <param name="userID">ID of the user whose records we're going to load.</param>
        /// <param name="pageNumber">the number of the page we're trying to load.</param>
        /// <param name="pageSize">Count of Elements in each page.</param>
        /// <returns></returns>
        public async Task<APIResponse<PagedResult<URLDTO>>> GetProfileURLList(int userID, int pageNumber, int pageSize)
        {
            HttpRequestMessage req = new(HttpMethod.Get, $"api/Users/Profile/{userID}?pageNumber={pageNumber}&pageSize={pageSize}");

            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<PagedResult<URLDTO>>? result = await JsonSerializer.DeserializeAsync<APIResponse<PagedResult<URLDTO>>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return result!;
        }
    }
}
