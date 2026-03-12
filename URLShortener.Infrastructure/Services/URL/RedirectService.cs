using AutoMapper;
using Microsoft.EntityFrameworkCore;
using URLShortener.Application.DTOs;
using URLShortener.Application.Interfaces.Infrastructure.External;
using URLShortener.Application.Interfaces.Services.URL;
using URLShortener.Application.Repositories;
using URLShortener.Domain.Entities.URL;
using URLShortenerAPI.Utility.Exceptions;

namespace URLShortener.Infrastructure.Services.URL
{
    public class RedirectService : IRedirectService
    {
        private readonly ICacheService _cacheService;
        private readonly IQueueService _redisQueueService;
        private readonly IMapper _mapper;
        private readonly IURLRepository _urlRepository;

        public RedirectService(ICacheService cacheService, IQueueService redisQueueService, IMapper mapper, IURLRepository urlRepository)
        {
            _cacheService = cacheService;
            _redisQueueService = redisQueueService;
            _mapper = mapper;
            _urlRepository = urlRepository;
        }

        /// <summary>
        /// Resolves a URL from databases. first it tries to fetch it from Redis Cache, if it's there, retrieves and returns the result. if not, we query the database and then cache it in redis for later calls.
        /// </summary>
        /// <param name="shortCode">short code of the URL.</param>
        /// <param name="requestInfo">information about the incoming get request such as IP address, user agent etc.</param>
        /// <returns> a string containing the Long URL.</returns>
        public async Task<URLDTO> ResolveURL(string shortCode)
        {
            // first we try to retrieve URL from cache
            URLModel? urlRecord = await ResolveURLFromCacheAsync(shortCode);

            // If not found in cache, we query database and cache it in Redis.
            if (urlRecord == null)
            {
                urlRecord = await ResolveURLFromDatabaseAsync(shortCode);
                await _cacheService.SetAsync(shortCode, urlRecord);
            }

            return _mapper.Map<URLDTO>(urlRecord);
        }

        /// <summary>
        /// Checks if we have what user has entered.
        /// </summary>
        /// <param name="shortcode">user's input</param>
        /// <param name="requestInfo">information about user's request.</param>
        /// <returns> if found, an object containing info about the URL</returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<URLDTO> CheckURLExists(string shortcode, IncomingRequestInfo requestInfo)
        {
            // Attempt to resolve URL from cache
            URLModel? url = await ResolveURLFromCacheAsync(shortcode);

            // If URL is not found in cache, attempt to resolve from the database
            if (url == null)
            {
                url = await ResolveURLFromDatabaseAsync(shortcode);
            }

            // If URL is found in either cache or database, enqueue the request
            if (url != null)
            {
                requestInfo.URL = url;
                await _redisQueueService.EnqueueItem(requestInfo);
                return _mapper.Map<URLDTO>(url);
            }

            throw new NotFoundException($"{shortcode} Not Found.");
        }


        /// <summary>
        /// this function fetches the LongURL from cache if it's cached there.
        /// </summary>
        /// <param name="shortCode">ShortCode as the key for key/value pair.</param>
        /// <returns>a <see cref="URLModel"/> record cached in Redis.</returns>
        private async Task<URLModel?> ResolveURLFromCacheAsync(string shortCode)
        {
            return await _cacheService.GetValueAsync<URLModel>(shortCode);
        }
        /// <summary>
        /// fetches a URL from database by query.
        /// </summary>
        /// <param name="shortCode">ShortCode of the URL to fetch.</param>
        /// <returns>a <see cref="URLModel"/> record from database.</returns>
        /// <exception cref="NotFoundException"></exception>
        private async Task<URLModel> ResolveURLFromDatabaseAsync(string shortCode)
        {
            URLModel url = await _urlRepository.GetAsync(x => x.ShortCode == shortCode, i => i.Include(r => r.Categories))
                    ?? throw new NotFoundException($"URL {shortCode} does not exist.");
            return url;
        }
    }
}