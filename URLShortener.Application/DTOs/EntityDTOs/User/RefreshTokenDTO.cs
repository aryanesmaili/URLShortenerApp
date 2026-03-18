namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed class RefreshTokenDTO
{
    public int Id { get; set; }
    public required string Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
}

