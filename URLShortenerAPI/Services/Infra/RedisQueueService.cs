using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Services.Utility
{
    internal class RedisQueueService : IRedisQueueService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly JsonSerializerOptions _serializerOptions;

        public RedisQueueService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
            _serializerOptions = new()
            { ReferenceHandler = ReferenceHandler.Preserve };
        }
        /// <summary>
        /// Adds an item to the Redis Queue.
        /// </summary>
        /// <typeparam name="T">Type of the item to be added.</typeparam>
        /// <param name="Data">the object to be added.</param>
        /// <returns></returns>
        public async Task EnqueueItem<T>(T Data) where T : class
        {
            // Serialize info to JSON
            var infoJason = JsonSerializer.Serialize(Data, _serializerOptions);

            // Push the serialized info onto a Redis list (queue)
            await _db.ListLeftPushAsync(typeof(T).Name.ToLower(), infoJason);
        }
        /// <summary>
        /// pulls an item out of the Redis queue.
        /// </summary>
        /// <typeparam name="T">type of the data you ask.</typeparam>
        /// <returns>an item of type <typeparamref name="T"/> pulled from redis queue. </returns>
        public async Task<T?> DequeueItem<T>() where T : class
        {
            // Get Item from Queue
            string? itemJson = await _db.ListRightPopAsync(typeof(T).Name.ToLower());

            if (itemJson == null)
            {
                return null;
            }
            // Deserialize and return Item
            return JsonSerializer.Deserialize<T>(itemJson, _serializerOptions);
        }
    }
}
