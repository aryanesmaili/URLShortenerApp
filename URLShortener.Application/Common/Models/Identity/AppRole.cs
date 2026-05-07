using Microsoft.AspNetCore.Identity;

namespace URLShortener.Application.Common.Models.Identity;

public sealed class AppRole : IdentityRole<long>
{
    public string? Description { get; set; }
}
