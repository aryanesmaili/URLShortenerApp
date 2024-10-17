namespace URLShortenerAPI.Services.Interfaces
{
    public interface ICacheService
    {
        public Task SetAsync<T>(string key, T value);
        public Task<List<T>?> GetAllValuesAsync<T>() where T : class;
        public Task<T?> GetValueAsync<T>(string key) where T : class;
        public Task RemoveAsync<T>(string key);
    }
}
