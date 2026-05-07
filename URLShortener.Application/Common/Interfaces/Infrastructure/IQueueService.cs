namespace URLShortener.Application.Common.Interfaces.Infrastructure
{
    public interface IQueueService
    {
        Task<T?> DequeueItem<T>() where T : class;
        Task EnqueueItem<T>(T Data) where T : class;
    }
}