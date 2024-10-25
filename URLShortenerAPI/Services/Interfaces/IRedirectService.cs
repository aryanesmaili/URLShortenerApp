using SharedDataModels.DTOs;
using URLShortenerAPI.Data.Entities.Analytics;

namespace URLShortenerAPI.Services.Interfaces
{
    public interface IRedirectService
    {
        public Task<bool> QuickLookup(string shortcode);
        public Task<URLDTO> ResolveURL(string shortCode, IncomingRequestInfo requestInfo);
    }
}
