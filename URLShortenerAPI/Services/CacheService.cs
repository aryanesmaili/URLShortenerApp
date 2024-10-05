using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Services
{
    internal class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        public CacheService(IDistributedCache cache, IConnectionMultiplexer redis)
        {
            _cache = cache;
            _redis = redis;
        }

        public async Task SetAsync<T>(string type, string key, T value)
        {
            var serializedData = JsonSerializer.Serialize(value);
            var content = Encoding.UTF8.GetBytes(serializedData);

            await _cache.SetAsync(type + "_" + key, content, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });
        }

        public async Task<List<T>?> GetAllValuesAsync<T>(string type) where T : class
        {
            var redisKeys = _redis
                .GetServer("localhost", 9191)
                .Keys(pattern: type + "_")
                .AsQueryable().Select(p => p.ToString()).ToList();

            var result = new List<T>();
            foreach (var redisKey in redisKeys)
            {
                result.Add(JsonSerializer.Deserialize<T>(await _cache.GetStringAsync(redisKey)));
            }
            return result;
        }

        public async Task<T?> GetValueAsync<T>(string type, string key) where T : class
        {
            var cachedData = await _cache.GetStringAsync($"{type}_{key}");

            if (cachedData == null)
                return null;

            return JsonSerializer.Deserialize<T>(cachedData);
        }
        public async Task RemoveAsync(string type, string key)
        {
            await _cache.RemoveAsync($"{type}_{key}");
        }
    }
}
