namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed record CheckResetPasswordCodeRequest
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

