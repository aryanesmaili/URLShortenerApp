using System.Text;
using System.Text.Json;
using URLShortener.Application.Interfaces.Services.Request;
using URLShortener.Domain.Entities.ClickInfo;

namespace URLShortener.Infrastructure.Services.Infra
{
    public class UserAgentService : IUserAgentService
    {
        private readonly HttpClient _httpClient;
        private const string _postAPIURL = "https://api.apicagent.com";
        public UserAgentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DeviceInfo?> GetRequestInfo(string userAgent)
        {
            HttpContent content = new StringContent(userAgent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(_postAPIURL, content);
            DeviceInfo? result = await JsonSerializer.DeserializeAsync<DeviceInfo>(await response.Content.ReadAsStreamAsync());

            return result;
        }
    }
}
