using URLShortenerAPI.Data.Entities.ClickInfo;

namespace URLShortenerAPI.Data.Interfaces.Infra
{
    internal interface IUserAgentService
    {
        Task<DeviceInfo?> GetRequestInfo(string userAgent);
    }
}