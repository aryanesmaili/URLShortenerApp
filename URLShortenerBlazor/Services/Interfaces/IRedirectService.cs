using SharedDataModels.DTOs;
using SharedDataModels.Responses;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IRedirectService
    {
        public Task<APIResponse<URLDTO>> ResolveURL(string shortcode);
    }
}