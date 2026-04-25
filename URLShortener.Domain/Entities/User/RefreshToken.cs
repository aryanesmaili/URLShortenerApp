namespace URLShortener.Domain.Entities.User;

public sealed class RefreshToken
{
    public long ID { get; set; }
    public required string TokenHash { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Revoked { get; set; }
    public bool IsActive => Revoked == null && !IsExpired;
    public bool IsExpired => DateTime.UtcNow >= Expires;

    public required long IdentityUserId { get; set; }
}
