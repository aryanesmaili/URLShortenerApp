using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.Features.URLs.DTOs;

public record URLCreateDTOBase
{
    private string longURL = string.Empty;

    [Required]
    public required string LongURL { get => longURL; init => longURL = value.Trim(); }
}