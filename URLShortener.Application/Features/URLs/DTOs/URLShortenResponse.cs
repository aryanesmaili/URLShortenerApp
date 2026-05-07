namespace URLShortener.Application.Features.URLs.DTOs;

public sealed class URLShortenResponse
{
    public required URLDTO URL { get; set; }
    public bool IsNew { get; set; }
}
