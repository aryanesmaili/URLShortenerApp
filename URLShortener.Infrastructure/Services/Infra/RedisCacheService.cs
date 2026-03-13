using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Interfaces.Infrastructure.External;

namespace URLShortener.Infrastructure.Services.Infra
{
    public sealed class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly RedisConnectionCreds _connectionCreds;
        private readonly IDatabase _db;
        private readonly JsonSerializerOptions _serializerOptions;
        private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromDays(3);

        public RedisCacheService(IConnectionMultiplexer redis, RedisConnectionCreds connectionCreds)
        {
            _redis = redis;
            _connectionCreds = connectionCreds;
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
        public async Task SetRange<T>(List<T> items, Func<T, string> keySelector, TimeSpan? span = null, CacheStrategy strategy = CacheStrategy.NotExists)
        {
            if (items == null || items.Count == 0)
                return;

            span ??= DefaultCacheDuration;
            var when = MapStrategy(strategy);

            var prepared = new List<(string key, string content)>(items.Count);
            foreach (var item in items)
            {
                if (item == null)
                    continue;

                string uniqueValue = keySelector(item);
                if (string.IsNullOrEmpty(uniqueValue))
                    throw new ArgumentException("Key selector returned null or empty value for an item.");

                prepared.Add((GenerateRedisKey<T>(uniqueValue), JsonSerializer.Serialize(item, _serializerOptions)));
            }

            var batch = _db.CreateBatch();
            var tasks = prepared.Select(p => batch.StringSetAsync(p.key, p.content, span, when)).ToList();

            batch.Execute();
            await Task.WhenAll(tasks);
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
        public async Task<List<T>?> GetAllValuesAsync<T>() where T : class
        {
            var redisKeys = _redis
                .GetServer(_connectionCreds.Host, _connectionCreds.Port)
                .Keys(pattern: GetRedisPattern<T>())
                .Select(p => p.ToString())
                .ToList();

            var result = new List<T>();

            if (redisKeys.Count == 0)
                return result = [];

            // FIX: Batch all reads instead of awaiting each one sequentially (N round trips → 1 batch)
            var batch = _db.CreateBatch();
            var tasks = redisKeys.Select(k => batch.StringGetAsync(k)).ToList();
            batch.Execute();
            var values = await Task.WhenAll(tasks);

            foreach (var cachedValue in values)
            {
                if (string.IsNullOrEmpty(cachedValue))
                    continue;

                var deserializedObject = JsonSerializer.Deserialize<T>(cachedValue!, _serializerOptions);
                if (deserializedObject != null)
                    result.Add(deserializedObject);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<T?> GetValueAsync<T>(string key) where T : class
        {
            var cachedData = await _db.StringGetAsync(GenerateRedisKey<T>(key));

            if (string.IsNullOrEmpty(cachedData))
                return null;

            return JsonSerializer.Deserialize<T>(cachedData!, _serializerOptions);
        }

        /// <inheritdoc/>
        public async Task RemoveAsync<T>(string key)
        {
            await _db.KeyDeleteAsync(GenerateRedisKey<T>(key));
        }

        // FIX: Added wildcard so Redis SCAN pattern actually matches keys (e.g. "url_*" not "url_")
        private static string GetRedisPattern<T>() => typeof(T).Name.ToLower() + "_*";

        private static string GenerateRedisKey<T>(string key) => $"{typeof(T).Name.ToLower()}_{key}";
    }
}