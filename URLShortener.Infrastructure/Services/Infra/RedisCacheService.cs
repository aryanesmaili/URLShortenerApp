using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using URLShortener.Application.Interfaces.Infrastructure.External;

namespace URLShortener.Infrastructure.Services.Infra;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly JsonSerializerOptions _serializerOptions;
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromDays(3);
    private const string DefaultCollectionIdentifier = "all";

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
        _serializerOptions = new()
        { ReferenceHandler = ReferenceHandler.Preserve };
    }

    private static When MapStrategy(CacheStrategy strategy)
    {
        return strategy switch
        {
            CacheStrategy.Always => When.Always,
            CacheStrategy.Exists => When.Exists,
            CacheStrategy.NotExists => When.NotExists,
            _ => throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null)
        };
    }

    /// <inheritdoc/>
    public async Task SetRange<T>(IEnumerable<T> items, Func<T, string> keySelector, TimeSpan? span = null, CacheStrategy strategy = CacheStrategy.NotExists)
    {
        if (items == null)
            return;

        // we detect how long to store the values in cache
        span ??= DefaultCacheDuration;
        // we detect in what situation we add these values to cache
        var when = MapStrategy(strategy);

        var prepared = new List<(string key, string content)>(); // we store each item separately
        foreach (var item in items)
        {
            if (item == null)
                continue;

            string uniqueValue = keySelector(item); // get the unique value used as the key provided by func caller.
            if (string.IsNullOrEmpty(uniqueValue))
                throw new ArgumentException("Key selector returned null or empty value for an item.");

            prepared.Add((GenerateRedisKey<T>(uniqueValue), JsonSerializer.Serialize(item, _serializerOptions)));
        }

        var batch = _db.CreateBatch();
        var tasks = prepared.Select(p => batch.StringSetAsync(p.key, p.content, span, when)).ToList();

        batch.Execute();
        await Task.WhenAll(tasks); // we await until all set operations in batch are done
    }

    /// <inheritdoc/>
    public async Task SetCollectionAsync<T>(IEnumerable<T> items, string? extraIdentifier = null, TimeSpan? span = null, CacheStrategy strategy = CacheStrategy.NotExists)
    {
        if (items == null)
            return;

        span ??= DefaultCacheDuration;
        string collectionIdentifier = extraIdentifier ?? DefaultCollectionIdentifier;
        var key = GenerateRedisKey<T>(collectionIdentifier);
        var when = MapStrategy(strategy);

        var serialized = JsonSerializer.Serialize(items, _serializerOptions);

        await _db.StringSetAsync(key, serialized, span, when);
    }


    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan? cacheDuration = null, CacheStrategy strategy = CacheStrategy.NotExists)
    {
        cacheDuration ??= DefaultCacheDuration;

        string serializedData = JsonSerializer.Serialize(value, _serializerOptions);
        string cacheKey = GenerateRedisKey<T>(key);
        var when = MapStrategy(strategy);

        await _db.StringSetAsync(cacheKey, serializedData, cacheDuration.Value, when);
    }



    /// <inheritdoc/>
    public async Task<T?> GetValueAsync<T>(string key)
    {
        var cachedData = await _db.StringGetAsync(GenerateRedisKey<T>(key));

        if (cachedData.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(cachedData!, _serializerOptions);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<T>?> GetCollectionAsync<T>(string? extraIdentifier = null)
    {
        string collectionIdentifier = extraIdentifier ?? DefaultCollectionIdentifier;
        var cachedData = await _db.StringGetAsync(GenerateRedisKey<T>(collectionIdentifier));

        if (cachedData.IsNullOrEmpty)
            return [];

        return JsonSerializer.Deserialize<List<T>>(cachedData!, _serializerOptions);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync<T>(string key)
    {
        await _db.KeyDeleteAsync(GenerateRedisKey<T>(key));
    }

    private static string GenerateRedisKey<T>(string key) => $"{typeof(T).Name.ToLower()}:{key}".ToLowerInvariant();

}