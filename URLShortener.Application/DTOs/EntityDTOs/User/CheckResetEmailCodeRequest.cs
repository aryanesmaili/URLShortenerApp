namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed record CheckResetEmailCodeRequest
{
    public string NewEmail { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
}
