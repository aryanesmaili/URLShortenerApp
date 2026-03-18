namespace URLShortener.Application.DTOs.EntityDTOs.URL;

public sealed class URLSummaryDTO
{
    public int ID { get; set; }
    public string ShortCode { get; set; } = string.Empty;
    public string LongURL { get; set; } = string.Empty;
}
