namespace URLShortener.Application.Interfaces.Infrastructure.External
{
    public interface IQueueService
    {
        Task<T?> DequeueItem<T>() where T : class;
        Task EnqueueItem<T>(T Data) where T : class;
    }
}