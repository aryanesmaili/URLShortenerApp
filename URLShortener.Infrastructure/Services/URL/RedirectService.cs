using AutoMapper;
using Microsoft.EntityFrameworkCore;
using URLShortener.Application.DTOs;
using URLShortener.Application.Interfaces.Infrastructure.External;
using URLShortener.Application.Interfaces.Services.URL;
using URLShortener.Application.Repositories;
using URLShortener.Application.Utility.Exceptions;
using URLShortener.Domain.Entities.URL;

namespace URLShortener.Infrastructure.Services.URL;

public sealed class RedirectService : IRedirectService
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

    /// <inheritdoc/>
    public async Task<URLDTO> ResolveShortCode(string shortCode)
    {
        URLModel? urlRecord = await GetOrThrowAsync(shortCode, refreshCacheOnDBFetch: true);

        return _mapper.Map<URLDTO>(urlRecord); ;
    }

    /// <inheritdoc/>
    public async Task<URLDTO> CheckURLExists(string shortCode, IncomingRequestInfo requestInfo)
    {
        URLModel? url = await GetOrThrowAsync(shortCode);

        // If URL is found in either cache or database, enqueue the request so that its info is processed
        requestInfo.URL = url;
        await _redisQueueService.EnqueueItem(requestInfo);

        return _mapper.Map<URLDTO>(url); ;
    }

    /// <summary>
    /// Tries to fetch URL by its ShortCode from Cache first. if it doesn't exist there, it tries to fetch it from Database.
    /// if no such shortCode is found, we throw <see cref="NotFoundException"/> with error message.
    /// </summary>
    /// <param name="shortCode">shortCode to fetch URL by</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">thrown when URL is not found</exception>
    private async Task<URLModel> GetOrThrowAsync(string shortCode, bool refreshCacheOnDBFetch = false)
    {
        URLModel? url = await ResolveURLFromCacheAsync(shortCode);

        if (url == null)
        {
            url = await ResolveURLFromDatabaseAsync(shortCode);
            if (url != null && refreshCacheOnDBFetch)
                await _cacheService.SetAsync(shortCode, url);
        }

        if (url == null) // URL Is neither in cache nor database. meaning we don't have such shortCode at all
            throw new NotFoundException(nameof(URLModel), nameof(URLModel.ShortCode), shortCode);

        return url;
    }

    /// <summary>
    /// this function fetches the LongURL from cache if it's cached there.
    /// </summary>
    /// <param name="shortCode">shortCode as the key for key/value pair.</param>
    /// <returns>a <see cref="URLModel"/> record cached in Redis.</returns>
    private async Task<URLModel?> ResolveURLFromCacheAsync(string shortCode)
    {
        return await _cacheService.GetValueAsync<URLModel>(shortCode);
    }

    /// <summary>
    /// fetches a URL from database by query.
    /// </summary>
    /// <param name="shortCode">shortCode of the URL to fetch.</param>
    /// <returns>a <see cref="URLModel"/> record from database.</returns>
    /// <exception cref="NotFoundException"></exception>
    private async Task<URLModel?> ResolveURLFromDatabaseAsync(string shortCode)
    {
        URLModel? url = await _urlRepository.GetAsync(x => x.ShortCode == shortCode, i => i.Include(r => r.Categories));
        return url;
    }
}