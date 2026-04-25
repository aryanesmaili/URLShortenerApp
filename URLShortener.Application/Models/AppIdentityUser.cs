using Microsoft.AspNetCore.Identity;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Application.Models;

public sealed class AppIdentityUser : IdentityUser<long>
{
    public string? DisplayName { get; set; }
    public long DomainUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}