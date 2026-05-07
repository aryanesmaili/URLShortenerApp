using IPinfo;
using IPinfo.Models;
using Microsoft.Extensions.Options;
using URLShortener.Application.Common.Interfaces.Infrastructure;
using URLShortener.Application.Configuration;

namespace URLShortener.Infrastructure.Services.Infra;

public sealed class IPInfoService : IIPInfoService
{
    private readonly IPinfoClient client;

    public IPInfoService(IOptions<IpInfoSettings> settings)
    {
        var ipInfoSettings = settings.Value;
        client = new IPinfoClient
            .Builder()
            .AccessToken(ipInfoSettings.AccessToken)
            .Build();
    }

    /// <summary>
    /// fetches info about an IP Address from IPInfo.io.
    /// </summary>
    /// <param name="IPAddress">IP Address to be checked</param>
    /// <returns>a <see cref="IPResponse"/> object containing info from IPInfo.io. </returns>
    public async Task<IPResponse> GetIPDetailsAsync(string IPAddress)
    {
        var response = await client.IPApi.GetDetailsAsync(IPAddress);
        return response;
    }
}
