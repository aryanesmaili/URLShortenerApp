namespace URLShortenerAPI.Data.Interfaces.Infra
{
    public interface IRedisQueueService
    {
        public Task<T?> DequeueItem<T>() where T : class;
        public Task EnqueueItem<T>(T Data) where T : class;
    }
}