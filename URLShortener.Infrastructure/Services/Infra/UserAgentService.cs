using System.Text;
using System.Text.Json;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Interfaces.Infrastructure.RequestProcessing;
using URLShortener.Domain.Entities.ClickInfo;

namespace URLShortener.Infrastructure.Services.Infra;

public sealed class UserAgentService : IUserAgentService
{
    private readonly HttpClient _httpClient;
    private readonly UserAgentServiceCreds _userAgentServiceCreds;

    public UserAgentService(HttpClient httpClient, UserAgentServiceCreds userAgentServiceCreds)
    {
        _httpClient = httpClient;
        _userAgentServiceCreds = userAgentServiceCreds;
    }

    /// <inheritdoc/>
    public async Task<DeviceInfo?> GetUserAgentInfo(string userAgent)
    {
        HttpContent content = new StringContent(userAgent, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync(_userAgentServiceCreds.APIAddress, content);
        DeviceInfo? result = await JsonSerializer.DeserializeAsync<DeviceInfo>(await response.Content.ReadAsStreamAsync());

        return result;
    }
}
