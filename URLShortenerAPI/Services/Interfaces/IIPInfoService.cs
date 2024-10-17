using IPinfo.Models;

namespace URLShortenerAPI.Services.Interfaces
{
    public interface IIPInfoService
    {
        public Task<IPResponse> GetIPDetailsAsync(string IPAddress);
    }
}
