namespace URLShortener.Application.Common.Interfaces.Infrastructure;

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
    /// Gets a Value from Redis Cache based on Type_Key:Value format (e.g URL_a62b53:Value).
    /// </summary>
    /// <typeparam name="T">Type of Data to be Deserialized into.</typeparam>
    /// <param name="key">Unique Identifier of the record (in case of URLs, it's ShortCode).</param>
    /// <returns></returns>
    Task<T?> GetValueAsync<T>(string key);

    /// <summary>
    /// Removes a data from redis Cache.
    /// </summary>
    /// <param name="key">Unique Identifier of the record (in case of URLs, it's ShortCode).</param>
    /// <returns></returns>
    Task RemoveAsync<T>(string key);

    /// <summary>
    /// Adds Batch of items to the Cache each as a separate key,value
    /// </summary>
    /// <typeparam name="T">type of value to be added</typeparam>
    /// <param name="items">list of items to be added</param>
    /// <param name="keySelector">the property value to store in cache</param>
    /// <param name="span">how long the item exists in cache. defaults to 3 days if not provided</param>
    /// <param name="strategy">In what situation the item is added to Cache</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"> thrown if key selector returns null or empty</exception>
    Task SetRange<T>(IEnumerable<T> items, Func<T, string> keySelector, TimeSpan? span = null, CacheStrategy strategy = CacheStrategy.NotExists);

    /// <summary>
    /// Adds Batch of items to the Cache each as one key with collection of values.
    /// </summary>
    /// <typeparam name="T">type of value to be added</typeparam>
    /// <param name="items">list of items to be added</param>
    /// <param name="extraIdentifier">extra identifier used in key. example key: URL:extraIdentifier</param>
    /// <param name="span">how long the item exists in cache. defaults to 3 days if not provided</param>
    /// <param name="strategy">In what situation the item is added to Cache</param>
    /// <returns></returns>
    Task SetCollectionAsync<T>(IEnumerable<T> items, string? extraIdentifier = null, TimeSpan? span = null, CacheStrategy strategy = CacheStrategy.NotExists);

    /// <summary>
    /// Gets Items set by <see cref="SetCollectionAsync{T}(IEnumerable{T}, string?, TimeSpan?, CacheStrategy)"/>.
    /// they are stored as key:[CollectionOfItems]
    /// </summary>
    /// <typeparam name="T">type of items to fetch</typeparam>
    /// <param name="extraIdentifier">extra identifier used in key. example key: URL:extraIdentifier</param>
    /// <returns></returns>
    Task<IReadOnlyCollection<T>?> GetCollectionAsync<T>(string? extraIdentifier = null);
}

public enum CacheStrategy
{
    Always,
    Exists,
    NotExists
}
