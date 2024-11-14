using IPinfo.Models;

namespace URLShortenerAPI.Data.Interfaces.Infra
{
    public interface IIPInfoService
    {
        public Task<IPResponse> GetIPDetailsAsync(string IPAddress);
    }
}
