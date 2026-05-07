namespace URLShortener.Application.Features.Users.DTOs;

public sealed record CheckResetPasswordCodeRequest
{
    private string email = string.Empty;
    public string Email
    {
        get => email;
        init => email = value.Trim();
    }

    private string token = string.Empty;
    public string Token
    {
        get => token;
        init => token = value.Trim();
    }

    private string newPassword = string.Empty;
    public string NewPassword
    {
        get => newPassword;
        init => newPassword = value.Trim();
    }
}

