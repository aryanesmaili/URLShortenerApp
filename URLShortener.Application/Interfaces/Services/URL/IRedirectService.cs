using URLShortener.Application.DTOs;

namespace URLShortener.Application.Interfaces.Services.URL
{
    public interface IRedirectService
    {
        Task<URLDTO> CheckURLExists(string shortcode, IncomingRequestInfo requestInfo);
        Task<URLDTO> ResolveURL(string shortCode);
    }
}
