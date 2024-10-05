using Microsoft.Extensions.Caching.Distributed;
using Npgsql.Internal;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using URLShortenerAPI.Services.Interfaces;
namespace URLShortenerAPI.Services
{
    internal class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly JsonSerializerOptions _serializerOptions;
        public CacheService(IDistributedCache cache, IConnectionMultiplexer redis)
        {
            _cache = cache;
            _redis = redis;
            _serializerOptions = new()
            { ReferenceHandler = ReferenceHandler.Preserve };
        }

        public async Task SetAsync<T>(string type, string key, T value)
        {

            var serializedData = JsonSerializer.Serialize(value, _serializerOptions);
            var content = Encoding.UTF8.GetBytes(serializedData);
            await _cache.SetAsync(type + "*" + key, content, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });
        }

        public async Task<List<T>?> GetAllValuesAsync<T>(string type) where T : class
        {
            // Get all Redis keys matching the provided pattern.
            var redisKeys = _redis
                .GetServer("localhost", 9191)
                .Keys(pattern: type + "")
                .AsQueryable()
                .Select(p => p.ToString())
                .ToList();

            var result = new List<T>();

            // Loop through each Redis key and get the corresponding value.
            foreach (var redisKey in redisKeys)
            {
                // Fetch the string value from the cache for the key.
                var cachedValue = await _cache.GetStringAsync(redisKey);

                if (!string.IsNullOrEmpty(cachedValue))
                {
                    // Deserialize the value into the specified type.
                    var deserializedObject = JsonSerializer.Deserialize<T>(cachedValue, _serializerOptions);
                    if (deserializedObject != null)
                    {
                        result.Add(deserializedObject);
                    }
                }
            }
            return result;
        }

        public async Task<T?> GetValueAsync<T>(string type, string key) where T : class
        {
            var cachedData = await _cache.GetStringAsync($"{type}_{key}");

            if (cachedData == null)
                return null;

            return JsonSerializer.Deserialize<T>(cachedData, _serializerOptions);
        }
        public async Task RemoveAsync(string type, string key)
        {
            await _cache.RemoveAsync($"{type}_{key}");
        }
    }
}
