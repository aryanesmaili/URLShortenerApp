namespace URLShortener.Application.Interfaces.Infrastructure.External
{
    public interface ICacheService
    {
        Task SetRange<T>(List<T> items, string propertyNameForKey);
        Task SetAsync<T>(string key, T value, TimeSpan CacheDuration = default);
        Task<List<T>?> GetAllValuesAsync<T>() where T : class;
        Task<T?> GetValueAsync<T>(string key) where T : class;
        Task RemoveAsync<T>(string key);
    }
}
