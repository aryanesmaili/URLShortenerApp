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
        
        public async Task<string> ResolveURL(string shortCode, IncomingRequestInfo requestInfo)
        {
            // first we try to retrieve URL from cache
            URLModel? result = await ResolveURLFromCacheAsync(shortCode);

            // If not found in cache, we query database and cache it in Redis
            if (result == null)
            {
                result = await ResolveURLFromDatabaseAsync(shortCode);
                await _cacheService.SetAsync("URL", shortCode, result);
            }

            ClickInfoModel clickInfo = new()
            {
                IPAddress = requestInfo.IPAddress,
                URLID = result.ID,
                URL = result,
                UserAgent = requestInfo.UserAgent,
                ClickedAt = DateTime.UtcNow,
            };

            // Analyze IP address and user agent for location and device info
            LocationInfo locationInfo = await AnalyzeIPAddress(requestInfo.IPAddress);
            DeviceInfo deviceInfo = AnalyzeUserAgent(requestInfo.UserAgent, requestInfo.Headers);

            // Relate locationInfo and deviceInfo to clickInfo
            clickInfo.PossibleLocation = locationInfo;
            clickInfo.DeviceInfo = deviceInfo;

            // Add click info, location, and device info to the database.
            await Task.WhenAll(
                _context.Clicks.AddAsync(clickInfo).AsTask(),
                _context.LocationInfos.AddAsync(locationInfo).AsTask(),
                _context.DeviceInfos.AddAsync(deviceInfo).AsTask()
                );

            // Save everything to database
            await _context.SaveChangesAsync();

            return result.LongURL;
        }

        private async Task<URLModel?> ResolveURLFromCacheAsync(string shortCode)
        {
            return await _cacheService.GetValueAsync<URLModel>("URL", shortCode);
        }

        private async Task<URLModel> ResolveURLFromDatabaseAsync(string shortCode)
        {
            URLModel url = await _context.URLs.FirstOrDefaultAsync(x => x.ShortCode == shortCode)
                    ?? throw new NotFoundException($"URL {shortCode} does not exist.");
            return url;
        }

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
        private static DeviceInfo AnalyzeUserAgent(string userAgent, Dictionary<string, string?> headers)
        {
            DeviceDetector.SetVersionTruncation(VersionTruncation.VERSION_TRUNCATION_NONE);
            var clientHints = ClientHints.Factory(headers);
            var deviceDetector = new DeviceDetector(userAgent, clientHints);
            deviceDetector.SetCache(new DictionaryCache());
            deviceDetector.Parse();

            var clientInfo = deviceDetector.GetClient().ToString();
            var osInfo = deviceDetector.GetOs().ToString();
            string? deviceInfo = deviceDetector.GetDeviceName().ToString();
            var brand = deviceDetector.GetBrandName().ToString();
            var model = deviceDetector.GetModel().ToString();
            var isBot = deviceDetector.IsBot();
            var botInfo = deviceDetector.GetBot().ToString();
            DeviceInfo info = new()
            { Brand = brand, ClientInfo = clientInfo, OS = osInfo, IsBot = isBot, Device = deviceInfo, BotInfo = botInfo, Model = model };
            return info;
        }
    }
}