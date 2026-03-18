using SharedDataModels.Responses;
using URLShortener.Application.DTOs.EntityDTOs.URL;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IRedirectService
    {
        Task<APIResponse<URLDTO>> ResolveURL(string shortcode);
    }
}