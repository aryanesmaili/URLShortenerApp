using IPinfo.Models;

namespace URLShortener.Application.Common.Interfaces.Infrastructure;

public interface IIPInfoService
{
    Task<IPResponse> GetIPDetailsAsync(string IPAddress);
}
