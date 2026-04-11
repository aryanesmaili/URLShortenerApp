namespace URLShortener.Application.DTOs.Settings;

public sealed record SMTPSettings
{
    public required string Server { get; init; }
    public int Port { get; init; }
    public required string SenderName { get; init; }
    public required string SenderEmail { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}

