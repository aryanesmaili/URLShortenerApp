namespace URLShortenerAPI.Services.Interfaces
{
    public interface ICacheService
    {
        public Task SetAsync<T>(string type, string key, T value);
        public Task<List<T>?> GetAllValuesAsync<T>(string type) where T : class;
        public Task<T?> GetValueAsync<T>(string type, string key) where T : class;
        public Task RemoveAsync(string type, string key);
    }
}
