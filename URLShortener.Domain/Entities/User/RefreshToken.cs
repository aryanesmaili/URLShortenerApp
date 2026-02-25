namespace URLShortener.Domain.Entities.User;

public class RefreshToken
{
    public int Id { get; set; }
    public required string Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Revoked { get; set; }
    public bool IsActive => Revoked == null && !IsExpired;
    public bool IsExpired => DateTime.UtcNow >= Expires;

    public required int UserId { get; set; }
    public required UserModel User { get; set; }
}
