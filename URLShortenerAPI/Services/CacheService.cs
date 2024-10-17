using Microsoft.Extensions.Caching.Distributed;
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
        /// <summary>
        /// Sets a value in Redis Cache in type_Key:Value format (e.g URL_a62b53:Value).
        /// </summary>
        /// <typeparam name="T"> type of value to be added to cache.</typeparam>
        /// <param name="key">key of the key:value pair.</param>
        /// <param name="value">value to be stored in key:value pair in Redis.</param>
        /// <returns></returns>
        public async Task SetAsync<T>(string key, T value)
        {

            var serializedData = JsonSerializer.Serialize(value, _serializerOptions);
            var content = Encoding.UTF8.GetBytes(serializedData);
            await _cache.SetAsync(typeof(T).Name.ToLower() + "_" + key, content, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });
        }
        /// <summary>
        /// gets all data saved in Redis cache.
        /// </summary>
        /// <param name="type">type of data that was stored (e.g URL, User data etc.)</param>
        /// <returns></returns>
        public async Task<List<T>?> GetAllValuesAsync<T>() where T : class
        {
            // Get all Redis keys matching the provided pattern.
            var redisKeys = _redis
                .GetServer("localhost", 9191)
                .Keys(pattern: typeof(T).Name.ToLower() + "_")
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
        /// <summary>
        /// Gets a Value from Redis Cache based on Type_Key:Value format (e.g URL_a62b53:Value).
        /// </summary>
        /// <typeparam name="T">Type of Data to be Deserialized into.</typeparam>
        /// <param name="key">Unique Identifier of the record (in case of URLs, it's ShortCode).</param>
        /// <returns></returns>
        public async Task<T?> GetValueAsync<T>(string key) where T : class
        {
            var cachedData = await _cache.GetStringAsync($"{typeof(T).Name.ToLower()}_{key}");

            if (cachedData == null)
                return null;

            return JsonSerializer.Deserialize<T>(cachedData, _serializerOptions);
        }
        /// <summary>
        /// Removes a data from redis Cache.
        /// </summary>
        /// <param name="key">Unique Identifier of the record (in case of URLs, it's ShortCode).</param>
        /// <returns></returns>
        public async Task RemoveAsync<T>(string key)
        {
            await _cache.RemoveAsync($"{typeof(T).Name.ToLower()}_{key}");
        }
    }
}
