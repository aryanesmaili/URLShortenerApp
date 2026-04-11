using IPinfo.Models;

namespace URLShortener.Application.Interfaces.Infrastructure.RequestProcessing;

public interface IIPInfoService
{
    Task<IPResponse> GetIPDetailsAsync(string IPAddress);
}
