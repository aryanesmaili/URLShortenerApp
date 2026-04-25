using Microsoft.AspNetCore.Identity;

namespace URLShortener.Application.Models;

public sealed class AppRole : IdentityRole<long>
{
    public string? Description { get; set; }
}
