using IPinfo;
using IPinfo.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;
using URLShortenerAPI.Data.Entities.Settings;
using URLShortenerAPI.Services.Interfaces;
namespace URLShortenerAPI.Services
{
    internal class IPInfoService : IIPInfoService
    {
        private readonly string myToken;
        readonly IPinfoClient client;
        public IPInfoService(IConfiguration settings)
        {
            myToken = settings["IPInfoApiKey"]!; // retrieve IPInfo.io key from user secrets.
            client = new IPinfoClient.Builder().AccessToken(myToken).Build();
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
}
