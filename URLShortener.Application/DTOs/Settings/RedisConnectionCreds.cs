namespace URLShortener.Application.DTOs.Settings
{
    public sealed class RedisConnectionCreds
    {
        public required string Host { get; set; }
        public int Port { get; set; }

    }
}
