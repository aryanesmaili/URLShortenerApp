namespace URLShortener.Application.Features.Users.DTOs;

public sealed record ChangeEmailRequest
{
    private string newEmail = string.Empty;
    public string NewEmail
    {
        get => newEmail;
        init => newEmail = value.Trim();
    }
}

