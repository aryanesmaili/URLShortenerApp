using IPinfo;
using IPinfo.Models;
using URLShortenerAPI.Services.Interfaces;
namespace URLShortenerAPI.Services
{
    internal class IPInfoService : IIPInfoService
    {
        private readonly static string myToken = "";
        readonly IPinfoClient client = new IPinfoClient.Builder().AccessToken(myToken).Build();
        
        public async Task<IPResponse> GetIPDetailsAsync(string IPAddress)
        {
            return await client.IPApi.GetDetailsAsync(IPAddress);
        }
    }
}
