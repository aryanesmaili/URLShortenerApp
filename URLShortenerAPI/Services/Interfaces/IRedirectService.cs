using URLShortenerAPI.Data.Entities.Analytics;

namespace URLShortenerAPI.Services.Interfaces
{
    public interface IRedirectService
    {
        public Task<string> ResolveURL(string shortCode, IncomingRequestInfo requestInfo);
    }
}
