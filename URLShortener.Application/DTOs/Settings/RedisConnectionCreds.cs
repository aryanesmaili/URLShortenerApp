namespace URLShortener.Application.DTOs.Settings
{
    public class RedisConnectionCreds
    {
        public required string Host { get; set; }
        public int Port { get; set; }

    }
}
