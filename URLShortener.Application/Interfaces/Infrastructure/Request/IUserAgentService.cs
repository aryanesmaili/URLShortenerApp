using URLShortener.Domain.Entities.ClickInfo;

namespace URLShortener.Application.Interfaces.Services.Request
{
    public interface IUserAgentService
    {
        /// <summary>
        /// Gets Info About the Device used in the request based on the provided user agent string.
        /// </summary>
        /// <param name="userAgent">user agent string used to </param>
        /// <returns></returns>
        Task<DeviceInfo?> GetUserAgentInfo(string userAgent);
    }
}