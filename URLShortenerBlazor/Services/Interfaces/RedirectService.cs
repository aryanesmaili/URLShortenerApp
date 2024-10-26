using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using System.Text.Json;

namespace URLShortenerBlazor.Services.Interfaces
{
    internal class RedirectService : IRedirectService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _authClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public RedirectService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _authClient = _httpClientFactory.CreateClient("Auth");
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Make property name matching case-insensitive
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Handle camelCase JSON property names
            };
        }

        public async Task<APIResponse<URLDTO>> ResolveURL(string shortcode)
        {
            HttpRequestMessage req = new(HttpMethod.Get, $"/Resolve/{shortcode}");

            HttpResponseMessage response = await _authClient.SendAsync(req);

            APIResponse<URLDTO>? result;

            result = await JsonSerializer.DeserializeAsync<APIResponse<URLDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return result!;
        }
    }
}
