namespace URLShortener.Application.Features.Users.DTOs;

public sealed record RefreshTokenDTO
{
    public long ID { get; init; }

    private string token = string.Empty;
    public required string Token
    {
        get => token;
        init => token = value.Trim();
    }

    public DateTime Expires { get; init; }
    public DateTime Created { get; init; }
}

