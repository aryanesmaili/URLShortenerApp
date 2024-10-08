using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using DeviceDetectorNET.Parser;
using Microsoft.EntityFrameworkCore;
using Pexita.Utility.Exceptions;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entities.Analytics;
using URLShortenerAPI.Data.Entities.ClickInfo;
using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Services
{
    internal class RedirectService : IRedirectService
    {
        private readonly AppDbContext _context;
        private readonly IIPInfoService _ipInfoService;
        private readonly ICacheService _cacheService;

        public RedirectService(AppDbContext context, IIPInfoService ipInfoService, ICacheService cacheService)
        {
            _context = context;
            _ipInfoService = ipInfoService;
            _cacheService = cacheService;
        }
        /// <summary>
        /// Resolves a URL from databases. first it tries to fetch it from Redis Cache, if it's there, retrieves and returns the result. if not, we query the database and then cache it in redis for later calls.
        /// </summary>
        /// <param name="shortCode">short code of the URL.</param>
        /// <param name="requestInfo">information about the incoming get request such as IP address, user agent etc.</param>
        /// <returns> a string containing the Long URL.</returns>
        public async Task<string> ResolveURL(string shortCode, IncomingRequestInfo requestInfo)
        {
            // first we try to retrieve URL from cache
            URLModel? urlRecord = await ResolveURLFromCacheAsync(shortCode);

            // If not found in cache, we query database and cache it in Redis.
            if (urlRecord == null)
            {
                urlRecord = await ResolveURLFromDatabaseAsync(shortCode);
                await _cacheService.SetAsync("URL", shortCode, urlRecord);
            }

            _context.Attach(urlRecord); // to prevent PostgreSQL from throwing duplicate PK Error.

            ClickInfoModel clickInfo = new()
            {
                IPAddress = requestInfo.IPAddress,
                URL = urlRecord,
                UserAgent = requestInfo.UserAgent,
                ClickedAt = DateTime.UtcNow,
            };

            // Analyze IP address and user agent for location and device info.
            LocationInfo locationInfo = await AnalyzeIPAddress(requestInfo.IPAddress);
            DeviceInfo deviceInfo = AnalyzeUserAgent(requestInfo.UserAgent, requestInfo.Headers);

            // Relate locationInfo and deviceInfo to clickInfo
            clickInfo.PossibleLocation = locationInfo;
            clickInfo.DeviceInfo = deviceInfo;

            // Add click info, location, and device info to the database.
            await _context.Clicks.AddAsync(clickInfo);
            await _context.LocationInfos.AddAsync(locationInfo);
            await _context.DeviceInfos.AddAsync(deviceInfo);

            // Save everything to database
            await _context.SaveChangesAsync();

            return urlRecord.LongURL;
        }
        /// <summary>
        /// this function fetches the LongURL from cache if it's cached there.
        /// </summary>
        /// <param name="shortCode">ShortCode as the key for key/value pair.</param>
        /// <returns>a <see cref="URLModel"/> record cached in Redis.</returns>
        private async Task<URLModel?> ResolveURLFromCacheAsync(string shortCode)
        {
            return await _cacheService.GetValueAsync<URLModel>("URL", shortCode);
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
        /// <summary>
        ///  sends the data to IPInfo.io and fetches the data analyzed by it.
        /// </summary>
        /// <param name="IP">IP Address to be analyzed.</param>
        /// <returns>a <see cref="LocationInfo"/> object containing info about the record.</returns>
        private async Task<LocationInfo> AnalyzeIPAddress(string IP)
        {
            var apiResponse = await _ipInfoService.GetIPDetailsAsync(IP);
            LocationInfo locationInfo = new()
            {
                City = apiResponse.City,
                Continent = apiResponse.Continent.Name,
                Country = apiResponse.CountryName,
                CountryCode = apiResponse.Country,
                Region = apiResponse.Region,
                Latitude = apiResponse.Latitude,
                Longitude = apiResponse.Longitude,
            };

            return locationInfo;
        }
        /// <summary>
        /// extracts data from user agent.
        /// </summary>
        /// <param name="userAgent">the user agent string.</param>
        /// <param name="headers">ClientHint info</param>
        /// <returns>a <see cref="DeviceInfo"/> object containing info about the user.</returns>
        private static DeviceInfo AnalyzeUserAgent(string userAgent, Dictionary<string, string?> headers)
        {
            DeviceDetector.SetVersionTruncation(VersionTruncation.VERSION_TRUNCATION_NONE); // to ensure I store the full version string not just minor/major
            var clientHints = ClientHints.Factory(headers);
            var deviceDetector = new DeviceDetector(userAgent, clientHints); // client hints provide additional information.
            deviceDetector.SetCache(new DictionaryCache()); // cache to increase performance.
            deviceDetector.Parse();

            var clientInfo = deviceDetector.GetClient().ToString();
            var osInfo = deviceDetector.GetOs().ToString();
            var brand = deviceDetector.GetBrandName().ToString();
            var model = deviceDetector.GetModel().ToString();
            var isBot = deviceDetector.IsBot();
            var botInfo = deviceDetector.GetBot().ToString();
            DeviceInfo info = new()
            { Brand = brand, ClientInfo = clientInfo, OS = osInfo, IsBot = isBot, BotInfo = botInfo, Model = model };
            return info;
        }
    }
}