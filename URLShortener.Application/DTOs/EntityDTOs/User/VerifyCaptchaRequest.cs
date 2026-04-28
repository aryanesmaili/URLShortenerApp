namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed record VerifyCaptchaRequest
{
    public string Token { get; init; } = string.Empty;
}
