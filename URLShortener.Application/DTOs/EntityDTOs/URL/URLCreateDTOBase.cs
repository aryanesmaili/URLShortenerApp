using System.ComponentModel.DataAnnotations;

namespace URLShortener.Application.DTOs.EntityDTOs.URL;

public record URLCreateDTOBase
{
    [Required]
    public required string LongURL { get; init; }
}