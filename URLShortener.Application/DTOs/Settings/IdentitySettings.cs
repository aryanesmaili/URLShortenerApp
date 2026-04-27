namespace URLShortener.Application.DTOs.Settings;

public sealed record IdentitySettings
{
    public IdentityPasswordSettings Password { get; init; } = new();

    public IdentityLockoutSettings Lockout { get; init; } = new();

    public IdentityUserSettings User { get; init; } = new();
}

public sealed record IdentityPasswordSettings
{
    public int RequiredLength { get; init; } = 6;

    public bool RequireNonAlphanumeric { get; init; }

    public bool RequireDigit { get; init; }

    public bool RequireLowercase { get; init; }

    public bool RequireUppercase { get; init; }

    public int RequiredUniqueChars { get; init; } = 1;
}

public sealed record IdentityLockoutSettings
{
    public int MaxFailedAccessAttempts { get; init; } = 5;

    public int DefaultLockoutMinutes { get; init; } = 5;

    public bool AllowedForNewUsers { get; init; } = true;
}

public sealed record IdentityUserSettings
{
    public bool RequireUniqueEmail { get; init; } = true;

    public bool RequireConfirmedEmail { get; init; }
}
