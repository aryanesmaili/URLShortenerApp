namespace URLShortenerAPI.Data.Interfaces.Infra
{
    public interface ICacheService
    {
        public Task SetRange<T>(List<T> items, string propertyNameForKey);
        public Task SetAsync<T>(string key, T value, TimeSpan CacheDuration = default);
        public Task<List<T>?> GetAllValuesAsync<T>() where T : class;
        public Task<T?> GetValueAsync<T>(string key) where T : class;
        public Task RemoveAsync<T>(string key);
    }
}
