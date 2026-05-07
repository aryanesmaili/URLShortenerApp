namespace URLShortener.Application.Features.Users.DTOs;

public sealed record UserUpdateDTO
{
    private string? name;
    public string? Name
    {
        get => name;
        init => name = value?.Trim();
    }

    private string? username;
    public string? Username
    {
        get => username;
        init => username = value?.Trim();
    }
}

