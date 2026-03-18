namespace URLShortener.Application.DTOs.EntityDTOs.URL;

public sealed class URLShortenResponse
{
    public required URLDTO URL { get; set; }
    public bool IsNew { get; set; }
}
