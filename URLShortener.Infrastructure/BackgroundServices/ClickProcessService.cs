using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using URLShortener.Application.Common.Interfaces.Infrastructure;
using URLShortener.Application.Common.Models;
using URLShortener.Application.Features.URLs.Interfaces.Repositories;
using URLShortener.Domain.Entities.ClickInfo;
using URLShortenerAPI.Data;

namespace URLShortener.Infrastructure.BackgroundServices;

public sealed class ClickProcessService : BackgroundService
{
    private readonly IQueueService _redisQueueService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IIPInfoService _ipInfoService;
    private readonly IUserAgentService _userAgentService;

    public ClickProcessService(IQueueService redisQueueService, IServiceProvider serviceProvider, IIPInfoService ipInfoService, IUserAgentService userAgentService)
    {
        _redisQueueService = redisQueueService;
        _serviceProvider = serviceProvider;
        _ipInfoService = ipInfoService;
        _userAgentService = userAgentService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            IncomingRequestMetadata? itemToProcess = await _redisQueueService.DequeueItem<IncomingRequestMetadata>();

            if (itemToProcess == null) // means the queue is empty.
            {
                await Task.Delay(30000, stoppingToken);
                continue;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var clickRepo = scope.ServiceProvider.GetRequiredService<IClickInfoRepository>();
                await ProcessClick(itemToProcess!, dbContext, clickRepo);
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
    public async Task ProcessClick(IncomingRequestMetadata requestInfo, AppDbContext context, IClickInfoRepository clickInfoRepository)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            if (context.Entry(requestInfo.URL!).State == EntityState.Detached)
            {
                context.Attach(requestInfo.URL!); // to avoid postgreSQL from throwing PK error.
            }
            var userAgentTask = AnalyzeUserAgent(requestInfo.UserAgent);
            var ipAddressTask = AnalyzeIPAddress(requestInfo.IPAddress);
            // create a new processed record 
            ClickInfoModel clickInfo = new()
            {
                IPAddress = requestInfo.IPAddress,
                URL = requestInfo.URL!,
                UserAgent = requestInfo.UserAgent,
                ClickedAt = DateTime.UtcNow,

            };

            await Task.WhenAll(userAgentTask, ipAddressTask);
            clickInfo.DeviceInfo = await userAgentTask;
            clickInfo.PossibleLocation = await ipAddressTask;

            requestInfo.URL!.ClickCount++;

            // Add click info, location, and device info to the database.
            clickInfoRepository.Add(clickInfo);

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
    private async Task<DeviceInfo?> AnalyzeUserAgent(string userAgent)
    {
        DeviceInfo? apiResponse = await _userAgentService.GetUserAgentInfo(userAgent);
        return apiResponse;
    }

}
