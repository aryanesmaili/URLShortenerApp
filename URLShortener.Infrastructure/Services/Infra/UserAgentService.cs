using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using URLShortener.Application.Common.Interfaces.Infrastructure;
using URLShortener.Application.Configuration;
using URLShortener.Domain.Entities.ClickInfo;

namespace URLShortener.Infrastructure.Services.Infra;

public sealed class UserAgentService : IUserAgentService
{
    private readonly HttpClient _httpClient;
    private readonly UserAgentServiceSettings _userAgentServiceSettings;

    public UserAgentService(HttpClient httpClient, IOptions<UserAgentServiceSettings> userAgentServiceSettings)
    {
        _httpClient = httpClient;
        _userAgentServiceSettings = userAgentServiceSettings.Value;
    }

    /// <inheritdoc/>
    public async Task<DeviceInfo?> GetUserAgentInfo(string userAgent)
    {
        HttpContent content = new StringContent(userAgent, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync(_userAgentServiceSettings.ApiAddress, content);
        DeviceInfo? result = await JsonSerializer.DeserializeAsync<DeviceInfo>(await response.Content.ReadAsStreamAsync());

        return result;
    }
}
