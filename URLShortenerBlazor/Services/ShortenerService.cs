using SharedDataModels.DTOs;
using Standart.Hash.xxHash;
using System.Net;
using System.Text;
using System.Text.Json;
using URLShortenerBlazor.Services.Interfaces;

namespace URLShortenerBlazor.Services
{
    public class ShortenerService : IShortenerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _authClient;
        public ShortenerService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _authClient = _httpClientFactory.CreateClient("Auth");
        }

        /// <summary>
        /// Sends the request to backend to shorten a single URL.
        /// </summary>
        /// <param name="createDTO">the <see cref="URLCreateDTO"/> object containing info about the URL to be shortened.</param>
        /// <returns>a <see cref="URLDTO"/> object showing the URL that was just added.</returns>
        /// <exception cref="NotAuthorizedException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<URLDTO> ShortenSingle(URLCreateDTO createDTO)
        {
            HttpRequestMessage req = new(HttpMethod.Post, "/api/URL/AddURL")
            {
                Content = new StringContent(JsonSerializer.Serialize(createDTO), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await _authClient.SendAsync(req);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,  // Make property name matching case-insensitive
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Handle camelCase JSON property names
                };
                return JsonSerializer.Deserialize<URLDTO>(content, options)!;
            }

            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new NotAuthorizedException(await response.Content.ReadAsStringAsync());
            }
            throw new Exception(await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Sends the request to backend to shorten a batch of URLs.
        /// </summary>
        /// <param name="createDTO">List of URLs to be shortened.</param>
        /// <returns>a <see cref="BatchURLAdditionResponse"/> object containing two lists: the URLs that already existed and those that are shortened now.</returns>
        /// <exception cref="NotAuthorizedException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<List<BatchURLAdditionResponse>> ShortenBatch(List<URLCreateDTO> createDTO)
        {
            HttpRequestMessage request = new(HttpMethod.Post, "/api/URL/AddBatchURL")
            {
                Content = new StringContent(JsonSerializer.Serialize(createDTO), Encoding.UTF8, "application/json"),
            };

            HttpResponseMessage response = await _authClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<List<BatchURLAdditionResponse>>(await response.Content.ReadAsStreamAsync())!;
            }

            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new NotAuthorizedException(await response.Content.ReadAsStringAsync());
            }
            throw new Exception(await response.Content.ReadAsStringAsync());
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
