namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed class UserLoginResponse
{
    public required UserDTO User { get; set; }
    public required RefreshTokenDTO RefreshToken { get; set; }
    public required string JWToken { get; set; }
}

