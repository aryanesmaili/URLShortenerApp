using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using DeviceDetectorNET.Parser;
using Microsoft.EntityFrameworkCore;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entities.Analytics;
using URLShortenerAPI.Data.Entities.ClickInfo;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Services
{
    internal class ClickProcessService : BackgroundService
    {
        private readonly IRedisQueueService _redisQueueService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IIPInfoService _ipInfoService;

        public ClickProcessService(IRedisQueueService redisQueueService, IServiceProvider serviceProvider, IIPInfoService ipInfoService)
        {
            _redisQueueService = redisQueueService;
            _serviceProvider = serviceProvider;
            _ipInfoService = ipInfoService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                IncomingRequestInfo? itemToProcess = await _redisQueueService.DequeueItem<IncomingRequestInfo>();

                if (itemToProcess == null) // means the queue is empty.
                {
                    await Task.Delay(30000, stoppingToken);
                    continue;
                }

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await ProcessClick(itemToProcess!, dbContext);
                }

                // to process 20 items per minute to save hardware resources.
                await Task.Delay(3000, stoppingToken);
            }
        }

        /// <summary>
        /// Process Click Info provided by the service.
        /// </summary>
        /// <param name="requestInfo">Information about the click.</param>
        /// <returns></returns>
        public async Task ProcessClick(IncomingRequestInfo requestInfo, AppDbContext context)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                if (context.Entry(requestInfo.URL!).State == EntityState.Detached)
                {
                    context.Attach(requestInfo.URL!); // to avoid postgreSQL from throwing PK error.
                }

                ClickInfoModel clickInfo = new()
                {
                    IPAddress = requestInfo.IPAddress,
                    URL = requestInfo.URL!,
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
                await context.Clicks.AddAsync(clickInfo);
                await context.LocationInfos.AddAsync(locationInfo);
                await context.DeviceInfos.AddAsync(deviceInfo);

                // Save everything to database
                await context.SaveChangesAsync();

                // commit as transaction to ensure atomicity (all or nothing)
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of an error
                await transaction.RollbackAsync();

                // Handle or log the error
                throw new Exception("Error processing the click information", ex);
            }
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

            if (deviceDetector.IsBot())
            {
                return new DeviceInfo() { IsBot = true, BotInfo = deviceDetector.GetBot().ToString() };
            }

            var clientInfo = deviceDetector.GetClient().ToString();
            var osInfo = deviceDetector.GetOs().ToString();
            var brand = deviceDetector.GetBrandName().ToString();
            var model = deviceDetector.GetModel().ToString();
            DeviceInfo info = new()
            { Brand = brand, ClientInfo = clientInfo, OS = osInfo, Model = model, IsBot = false };
            return info;
        }

    }
}
