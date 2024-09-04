namespace URLShortenerAPI.Data.Entites.Settings
{
    internal class JwtSettings
    {
        public string? SecretKey { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int ExpiresInMinutes { get; set; }
    }
}
