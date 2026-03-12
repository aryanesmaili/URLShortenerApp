namespace URLShortener.Application.Interfaces.Infrastructure.External;

public interface ICacheService
{
    /// <summary>
    /// Sets a value in Redis Cache in type_Key:Value format (e.g URL_a62b53:Value).
    /// </summary>
    /// <typeparam name="T"> type of value to be added to cache.</typeparam>
    /// <param name="key">key of the key:value pair.</param>
    /// <param name="value">value to be stored in key:value pair in Redis.</param>
    /// <param name="cacheDuration">Duration to store the cached value. defaults to 3 days if not provided</param>
    /// <param name="strategy">In what situation the item is added to Cache</param>
    /// <returns></returns>
    Task SetAsync<T>(string key, T value, TimeSpan? cacheDuration = null, CacheStrategy strategy = CacheStrategy.NotExists);

    /// <summary>
    /// gets all data saved in Redis cache.
    /// </summary>
    /// <param name="type">type of data that was stored (e.g URL, User data etc.)</param>
    /// <returns></returns>
    Task<List<T>?> GetAllValuesAsync<T>() where T : class;

    /// <summary>
    /// Gets a Value from Redis Cache based on Type_Key:Value format (e.g URL_a62b53:Value).
    /// </summary>
    /// <typeparam name="T">Type of Data to be Deserialized into.</typeparam>
    /// <param name="key">Unique Identifier of the record (in case of URLs, it's ShortCode).</param>
    /// <returns></returns>
    Task<T?> GetValueAsync<T>(string key) where T : class;

    /// <summary>
    /// Removes a data from redis Cache.
    /// </summary>
    /// <param name="key">Unique Identifier of the record (in case of URLs, it's ShortCode).</param>
    /// <returns></returns>
    Task RemoveAsync<T>(string key);

    /// <summary>
    /// Adds Batch of items to the Cache
    /// </summary>
    /// <typeparam name="T">type of value to be added</typeparam>
    /// <param name="items">list of items to be added</param>
    /// <param name="keySelector">the property value to store in cache</param>
    /// <param name="span">how long the item exists in cache. defaults to 3 days if not provided</param>
    /// <param name="strategy">In what situation the item is added to Cache</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"> thrown if key selector returns null or empty</exception>
    Task SetRange<T>(List<T> items, Func<T, string> keySelector, TimeSpan? span = null, CacheStrategy strategy = CacheStrategy.NotExists);
}

public enum CacheStrategy
{
    Always,
    Exists,
    NotExists
}
