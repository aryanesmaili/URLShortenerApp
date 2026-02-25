using URLShortener.Domain.Entities.ClickInfo;

namespace URLShortener.Application.Interfaces.Services.Request
{
    public interface IUserAgentService
    {
        Task<DeviceInfo?> GetRequestInfo(string userAgent);
    }
}