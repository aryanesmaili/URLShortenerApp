namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed record ChangeEmailRequest
{
    private string? _email;
    public string NewEmail
    {
        get => _email ?? string.Empty;
        init => _email = value?.Trim();
    }
}

