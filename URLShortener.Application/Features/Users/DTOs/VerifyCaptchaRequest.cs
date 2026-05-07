namespace URLShortener.Application.Features.Users.DTOs;

public sealed record VerifyCaptchaRequest
{
    private string token = string.Empty;

    public string Token
    {
        get => token;
        init => token = value.Trim();
    }
}
