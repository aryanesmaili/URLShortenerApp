using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using Standart.Hash.xxHash;
using System.Net;
using System.Text;
using System.Text.Json;
using URLShortenerBlazor.Services.Interfaces;

namespace URLShortenerBlazor.Services
{
    internal class ShortenerService : IShortenerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _authClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ShortenerService(IHttpClientFactory httpClientFactory)
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

        /// <summary>
        /// Sends the request to backend to shorten a single URL.
        /// </summary>
        /// <param name="createDTO">the <see cref="URLCreateDTO"/> object containing info about the URL to be shortened.</param>
        /// <returns>a <see cref="URLDTO"/> object showing the URL that was just added.</returns>
        /// <exception cref="Exception"></exception>
        public async Task<APIResponse<URLShortenResponse>> ShortenSingle(URLCreateDTO createDTO)
        {
            HttpRequestMessage req = new(HttpMethod.Post, "/api/URL/AddURL")
            {
                Content = new StringContent(JsonSerializer.Serialize(createDTO), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await _authClient.SendAsync(req);

            APIResponse<URLShortenResponse>? responseContent;
            string x = string.Empty;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                responseContent = new() { ErrorType = ErrorType.NotAuthorizedException };

            else
                responseContent = await JsonSerializer.DeserializeAsync<APIResponse<URLShortenResponse>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return responseContent!;
        }

        /// <summary>
        /// Sends the request to backend to shorten a batch of URLs.
        /// </summary>
        /// <param name="createDTO">List of URLs to be shortened.</param>
        /// <returns>a <see cref="URLShortenResponse"/> object containing shortened URls and an indicator if they're new or old.</returns>
        /// <exception cref="Exception"></exception>
        public async Task<APIResponse<List<URLShortenResponse>>> ShortenBatch(List<URLCreateDTO> createDTO)
        {
            HttpRequestMessage request = new(HttpMethod.Post, "/api/URL/AddBatchURL")
            {
                Content = new StringContent(JsonSerializer.Serialize(createDTO), Encoding.UTF8, "application/json"),
            };

            HttpResponseMessage? response = await _authClient.SendAsync(request);

            APIResponse<List<URLShortenResponse>> result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                result = new() { ErrorType = ErrorType.NotAuthorizedException };

            else
                result = await JsonSerializer.DeserializeAsync<APIResponse<List<URLShortenResponse>>>(await response.Content.ReadAsStreamAsync()!, _jsonSerializerOptions)!
                    ?? new APIResponse<List<URLShortenResponse>>
                    {
                        Success = false,
                        ErrorMessage = "Failed to deserialize response."
                    }; ;

            return result;
        }

        /// <summary>
        /// Deletes a URL from Database
        /// </summary>
        /// <param name="urlID">ID of the URL to be deleted.</param>
        /// <returns>response about the operation.</returns>
        public async Task<APIResponse<string>> DeleteURL(int urlID)
        {
            HttpRequestMessage req = new(HttpMethod.Delete, $"/api/URL/Delete/{urlID}");

            HttpResponseMessage response = await _authClient.SendAsync(req);
            APIResponse<string>? result;
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                result = new() { ErrorType = ErrorType.NotAuthorizedException };
            else
                result = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return result!;
        }

        /// <summary>
        /// Activate or Deactivates a URL's condition based on it's current situation.
        /// </summary>
        /// <param name="urlID">ID Of the URL to be toggled.</param>
        /// <returns></returns>
        public async Task<APIResponse<string>> ToggleActivation(int urlID)
        {
            HttpRequestMessage req = new(HttpMethod.Post, $"/api/URL/ToggleActivation/{urlID}");

            HttpResponseMessage response = await _authClient.SendAsync(req);
            APIResponse<string>? result;
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                result = new() { ErrorType = ErrorType.NotAuthorizedException };
            else
                result = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return result!;
        }

        /// <summary>
        /// Generates a shortened-alike string to be shown on home screen.
        /// </summary>
        /// <param name="longURL">the url to be shortened.</param>
        /// <returns>a 6 character long string.</returns>
        public string FakeShortener(string longURL)
        {
            // Convert the URL into a byte array
            byte[] data = Encoding.UTF8.GetBytes(longURL);

            // Calculate the XXHash32 value
            uint hash = xxHash32.ComputeHash(data, data.Length);

            // Convert the hash to a base-36 string and truncate to 6 characters
            string shortUrl = hash.ToString("x").Substring(0, 6); // Use hex representation for the shortened URL

            return shortUrl;
        }
    }
}
