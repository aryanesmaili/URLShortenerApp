using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Models;
using URLShortener.Application.Repositories;
using URLShortener.Persistence.Repositories;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence;

public static class DependencyInjection
{
    private const string DefaultConnectionName = "DefaultConnection";

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DefaultConnectionName)
            ?? throw new InvalidOperationException($"Connection string '{DefaultConnectionName}' is missing.");

        // Register AppDbContext with both AddDbContext and AddDbContextFactory for different usage scenarios
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(connectionString));

        // Configure Identity options from settings
        services.AddSingleton<IConfigureOptions<IdentityOptions>, ConfigureIdentityOptionsFromSettings>();
        // Register Identity services with custom user and role types
        services.AddIdentity<AppIdentityUser, AppRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Register Repositories and Unit of Work to DI container
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IServiceTariffRepository, ServiceTariffRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}

/// <summary>
/// This Class configures IdentityOptions based on the values provided in IdentitySettings, which are typically loaded from configuration (e.g., appsettings.json). It maps the password, lockout, and user settings from IdentitySettings to the corresponding properties in IdentityOptions, allowing for centralized configuration of Identity behavior.
/// </summary>
/// <param name="identitySettings"></param>
internal sealed class ConfigureIdentityOptionsFromSettings(IOptions<IdentitySettings> identitySettings)
    : IConfigureOptions<IdentityOptions>
{
    public void Configure(IdentityOptions options)
    {
        var settings = identitySettings.Value;

        options.Password.RequiredLength = settings.Password.RequiredLength;
        options.Password.RequireNonAlphanumeric = settings.Password.RequireNonAlphanumeric;
        options.Password.RequireDigit = settings.Password.RequireDigit;
        options.Password.RequireLowercase = settings.Password.RequireLowercase;
        options.Password.RequireUppercase = settings.Password.RequireUppercase;
        options.Password.RequiredUniqueChars = settings.Password.RequiredUniqueChars;

        options.Lockout.MaxFailedAccessAttempts = settings.Lockout.MaxFailedAccessAttempts;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(settings.Lockout.DefaultLockoutMinutes);
        options.Lockout.AllowedForNewUsers = settings.Lockout.AllowedForNewUsers;

        options.User.RequireUniqueEmail = settings.User.RequireUniqueEmail;
        options.SignIn.RequireConfirmedEmail = settings.User.RequireConfirmedEmail;
    }
}
