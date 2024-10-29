using URLShortenerAPI.Data.Entities.ClickInfo;

namespace URLShortenerAPI.Services.Interfaces
{
    internal interface IUserAgentService
    {
        Task<DeviceInfo?> GetRequestInfo(string userAgent);
    }
}