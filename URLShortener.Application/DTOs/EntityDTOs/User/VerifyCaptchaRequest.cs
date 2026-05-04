namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed record VerifyCaptchaRequest
{
    private string token = string.Empty;

    public string Token
    {
        get => token;
        init => token = value.Trim();
    }
}
