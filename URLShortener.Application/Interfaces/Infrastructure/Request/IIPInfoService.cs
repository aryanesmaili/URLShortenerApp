using IPinfo.Models;

namespace URLShortener.Application.Interfaces.Services.Request
{
    public interface IIPInfoService
    {
        Task<IPResponse> GetIPDetailsAsync(string IPAddress);
    }
}
