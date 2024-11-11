using SharedDataModels.DTOs;
using URLShortenerAPI.Data.Entities.Analytics;

namespace URLShortenerAPI.Services.Interfaces.URLRelated
{
    public interface IRedirectService
    {
        public Task<URLDTO> CheckURLExists(string shortcode, IncomingRequestInfo requestInfo);
        public Task<URLDTO> ResolveURL(string shortCode);
    }
}
