using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed class ChangeEmailRequest
{
    private string? _email;

    [EmailAddress(ErrorMessage = "Entered value is not a valid Email.")]
    public string NewEmail
    {
        get => _email ?? string.Empty;
        set => _email = value?.Trim();
    }
}

