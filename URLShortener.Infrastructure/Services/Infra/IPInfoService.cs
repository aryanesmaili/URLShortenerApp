using IPinfo;
using IPinfo.Models;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Interfaces.Infrastructure.RequestProcessing;

namespace URLShortener.Infrastructure.Services.Infra;

public sealed class IPInfoService : IIPInfoService
{
    private readonly IPinfoClient client;

    public IPInfoService(IPInfoCreds creds)
    {
        client = new IPinfoClient
            .Builder()
            .AccessToken(creds.AccessToken)
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
