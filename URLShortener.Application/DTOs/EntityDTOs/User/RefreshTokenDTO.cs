namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed class RefreshTokenDTO
{
    public long ID { get; set; }
    public required string Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
}

