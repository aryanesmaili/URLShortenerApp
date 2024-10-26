using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedDataModels.DTOs;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entities.Analytics;
using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Services.Interfaces;
using URLShortenerAPI.Utility.Exceptions;

namespace URLShortenerAPI.Services
{
    internal class RedirectService : IRedirectService
    {
        private readonly AppDbContext _context;
        private readonly IIPInfoService _ipInfoService;
        private readonly ICacheService _cacheService;
        private readonly IRedisQueueService _redisQueueService;
        private readonly IMapper _mapper;
        public RedirectService(AppDbContext context, IIPInfoService ipInfoService, ICacheService cacheService, IRedisQueueService redisQueueService, IMapper mapper)
        {
            _context = context;
            _ipInfoService = ipInfoService;
            _cacheService = cacheService;
            _redisQueueService = redisQueueService;
            _mapper = mapper;
        }

        /// <summary>
        /// Resolves a URL from databases. first it tries to fetch it from Redis Cache, if it's there, retrieves and returns the result. if not, we query the database and then cache it in redis for later calls.
        /// </summary>
        /// <param name="shortCode">short code of the URL.</param>
        /// <param name="requestInfo">information about the incoming get request such as IP address, user agent etc.</param>
        /// <returns> a string containing the Long URL.</returns>
        public async Task<URLDTO> ResolveURL(string shortCode, IncomingRequestInfo requestInfo)
        {
            // first we try to retrieve URL from cache
            URLModel? urlRecord = await ResolveURLFromCacheAsync(shortCode);

            // If not found in cache, we query database and cache it in Redis.
            if (urlRecord == null)
            {
                urlRecord = await ResolveURLFromDatabaseAsync(shortCode);
                await _cacheService.SetAsync(shortCode, urlRecord);
            }

            requestInfo.URL = urlRecord;

            await _redisQueueService.EnqueueItem(requestInfo);

            return _mapper.Map<URLDTO>(urlRecord);
        }

        public async Task<bool> QuickLookup(string shortcode)
        {
            URLModel? url;
            url = await ResolveURLFromCacheAsync(shortcode);
            if (url != null)
                return true;

            url = await ResolveURLFromDatabaseAsync(shortcode);
            if (url != null)
                return true;

            return false;
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
            URLModel url = await _context.URLs.FirstOrDefaultAsync(x => x.ShortCode == shortCode)
                    ?? throw new NotFoundException($"URL {shortCode} does not exist.");
            return url;
        }
    }
}