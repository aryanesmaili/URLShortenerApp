using SharedDataModels.Responses;
using URLShortener.Application.DTOs;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IRedirectService
    {
        Task<APIResponse<URLDTO>> ResolveURL(string shortcode);
    }
}