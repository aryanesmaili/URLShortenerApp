using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using URLShortener.Application.Configuration;

namespace URLShortenerAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddSingleton<IConfigureOptions<CorsOptions>, ConfigureCorsOptionsFromSettings>();
        services.AddSingleton<IConfigureOptions<AuthorizationOptions>, ConfigureAuthorizationOptionsFromSettings>();
        services.AddSingleton<IConfigureOptions<AntiforgeryOptions>, ConfigureAntiforgeryOptionsFromSettings>();
        services.AddSingleton<IConfigureNamedOptions<JwtBearerOptions>, ConfigureJwtBearerOptionsFromSettings>();
        services.AddSingleton<IConfigureOptions<RateLimiterOptions>, ConfigureRateLimiterOptionsFromSettings>();

        services.AddCors();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddAuthorizationBuilder();
        services.AddAntiforgery();
        services.AddRateLimiter(_ => { });
        services.AddSignalR();

        return services;
    }
}

internal sealed class ConfigureCorsOptionsFromSettings(IOptions<CorsSettings> corsSettings)
    : IConfigureOptions<CorsOptions>
{
    public void Configure(CorsOptions options)
    {
        var settings = corsSettings.Value;

        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(settings.AllowedOrigins);

            if (settings.AllowedHeaders.Length == 0 || settings.AllowedHeaders.Contains("*", StringComparer.Ordinal))
                policy.AllowAnyHeader();
            else
                policy.WithHeaders(settings.AllowedHeaders);

            if (settings.AllowedMethods.Length == 0 || settings.AllowedMethods.Contains("*", StringComparer.Ordinal))
                policy.AllowAnyMethod();
            else
                policy.WithMethods(settings.AllowedMethods);

            if (settings.AllowCredentials)
                policy.AllowCredentials();
        });
    }
}

internal sealed class ConfigureAuthorizationOptionsFromSettings(IOptions<AuthorizationSettings> authorizationSettings)
    : IConfigureOptions<AuthorizationOptions>
{
    public void Configure(AuthorizationOptions options)
    {
        foreach (var policy in authorizationSettings.Value.Policies)
        {
            options.AddPolicy(policy.Key, builder =>
            {
                builder.RequireAuthenticatedUser();
                builder.RequireRole(policy.Value);
            });
        }
    }
}

internal sealed class ConfigureAntiforgeryOptionsFromSettings(IOptions<AntiforgerySettings> antiforgerySettings)
    : IConfigureOptions<AntiforgeryOptions>
{
    public void Configure(AntiforgeryOptions options)
    {
        var settings = antiforgerySettings.Value;

        options.HeaderName = settings.HeaderName;
        options.Cookie.Name = settings.CookieName;
        options.Cookie.Path = settings.CookiePath;
        options.Cookie.HttpOnly = settings.CookieHttpOnly;
        options.Cookie.SameSite = Enum.Parse<SameSiteMode>(settings.CookieSameSite, ignoreCase: true);
        options.Cookie.SecurePolicy = Enum.Parse<CookieSecurePolicy>(settings.CookieSecurePolicy, ignoreCase: true);
    }
}

internal sealed class ConfigureJwtBearerOptionsFromSettings(
    IOptions<JwtSettings> jwtSettings,
    IOptions<JwtBearerSettings> jwtBearerSettings,
    IOptions<AuthenticationCookieSettings> authenticationCookieSettings)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(string? name, JwtBearerOptions options)
    {
        if (!string.Equals(name, JwtBearerDefaults.AuthenticationScheme, StringComparison.Ordinal))
            return;

        Configure(options);
    }

    public void Configure(JwtBearerOptions options)
    {
        var jwt = jwtSettings.Value;
        var bearer = jwtBearerSettings.Value;
        var cookies = authenticationCookieSettings.Value;
        var key = Encoding.ASCII.GetBytes(jwt.TokenSecretKey);

        options.RequireHttpsMetadata = bearer.RequireHttpsMetadata;
        options.SaveToken = bearer.SaveToken;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(cookies.JwtCookieName, out var token))
                    context.Token = token;

                return Task.CompletedTask;
            }
        };
    }
}

internal sealed class ConfigureRateLimiterOptionsFromSettings(IOptions<RateLimitingSettings> rateLimitingSettings)
    : IConfigureOptions<RateLimiterOptions>
{
    public void Configure(RateLimiterOptions options)
    {
        var settings = rateLimitingSettings.Value;
        options.RejectionStatusCode = settings.RejectionStatusCode;

        foreach (var policy in settings.Policies)
        {
            options.AddPolicy(policy.Key, context => BuildPolicy(policy.Key, policy.Value, context));
        }
    }

    private static RateLimitPartition<string> BuildPolicy(string policyName, RateLimitPolicySettings policy, HttpContext context)
    {
        var partitionKey = ResolvePartitionKey(context, policyName, policy.PartitionStrategy);
        var window = TimeSpan.FromSeconds(policy.WindowSeconds);

        return policy.Algorithm switch
        {
            "FixedWindow" => RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = policy.PermitLimit,
                Window = window
            }),
            "SlidingWindow" => RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = policy.PermitLimit,
                Window = window,
                SegmentsPerWindow = policy.SegmentsPerWindow
            }),
            _ => throw new InvalidOperationException($"Unsupported rate limiting algorithm '{policy.Algorithm}'.")
        };
    }

    private static string ResolvePartitionKey(HttpContext context, string policyName, string strategy)
    {
        return strategy switch
        {
            "AuthenticatedUser" => context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.Identity?.Name
                ?? "anonymous",
            "ClientIp" => context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => policyName
        };
    }
}
