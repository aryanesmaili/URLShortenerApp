using URLShortener.Application.DTOs;
using URLShortener.Application.DTOs.EntityDTOs.URL;

namespace URLShortener.Application.Interfaces.Services.URL
{
    public interface IRedirectService
    {

        /// <summary>
        /// Checks if we have what user has entered.
        /// </summary>
        /// <param name="shortCode">user's input</param>
        /// <param name="requestInfo">information about user's request.</param>
        /// <returns> if found, an object containing info about the URL</returns>
        Task<URLDTO> CheckURLExists(string shortcode, IncomingRequestInfo requestInfo);

        /// <summary>
        /// Resolves a URL from databases. first it tries to fetch it from Redis Cache, if it's there, retrieves and returns the result. if not, we query the database and then cache it in redis for later calls.
        /// </summary>
        /// <param name="shortCode">short code of the URL.</param>
        /// <param name="requestInfo">information about the incoming get request such as IP address, user agent etc.</param>
        /// <returns> a string containing the Long URL.</returns>
        Task<URLDTO> ResolveShortCode(string shortCode);
    }
}
