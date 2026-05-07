namespace URLShortener.Application.Features.URLs.DTOs;

public sealed record URLSummaryDTO
{
    public int ID { get; init; }

    private string shortCode = string.Empty;
    public string ShortCode
    {
        get => shortCode;
        init => shortCode = value.Trim();
    }

    private string longURL = string.Empty;
    public string LongURL
    {
        get => longURL;
        init => longURL = value.Trim();
    }
}
