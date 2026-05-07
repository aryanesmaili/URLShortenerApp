namespace URLShortener.Application.Features.Users.DTOs;

public sealed record CheckResetEmailCodeRequest
{
    private string newEmail = string.Empty;

    public string NewEmail
    {
        get => newEmail;
        init => newEmail = value.Trim();
    }

    private string token = string.Empty;
    public string Token
    {
        get => token;
        init => token = value.Trim();
    }
}
