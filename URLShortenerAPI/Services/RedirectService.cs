using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using DeviceDetectorNET.Parser;
using Microsoft.EntityFrameworkCore;
using Pexita.Utility.Exceptions;
using System.Security.Policy;
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
        public RedirectService(AppDbContext context, IIPInfoService ipInfoService)
        {
            _context = context;
            _ipInfoService = ipInfoService;
        }
        //TODO: Add caching to AddURL. add Refresh Cache here.
        public async Task<string> ResolveURL(string shortCode, IncomingRequestInfo requestInfo)
        {
            // first we try to retrieve URL from cache
            URLModel? result = await ResolveURLFromCacheAsync(shortCode);

            // If not found in cache, we query database
            result ??= await ResolveURLFromDatabaseAsync(shortCode);

            ClickInfoModel clickInfo = new()
            {
                IPAddress = requestInfo.IPAddress,
                URL = result,
                UserAgent = requestInfo.UserAgent,
                ClickedAt = DateTime.UtcNow,
                URLID = result.ID
            };

            // Analyze IP address and user agent for location and device info
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

            return result.LongURL;
        }

        private async Task<URLModel?> ResolveURLFromCacheAsync(string shortCode)
        {
            throw new NotImplementedException();
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