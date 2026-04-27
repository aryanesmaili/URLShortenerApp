using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Interfaces.Infrastructure.External;
using URLShortener.Application.Interfaces.Infrastructure.RequestProcessing;
using URLShortener.Application.Interfaces.Services.Payment;
using URLShortener.Application.Interfaces.Services.URL;
using URLShortener.Application.Interfaces.Services.User;
using URLShortener.Domain.Enums;
using URLShortener.Domain.Interfaces;
using URLShortener.Infrastructure.BackgroundServices;
using URLShortener.Infrastructure.Services.Infra;
using URLShortener.Infrastructure.Services.Payment;
using URLShortener.Infrastructure.Services.Payment.PaymentProviders;
using URLShortener.Infrastructure.Services.URL;
using URLShortener.Infrastructure.Services.User;

namespace URLShortener.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationSettings(configuration);
        services.AddHttpClient();
        services.AddRedisInfrastructure();

        services.AddHostedService<ClickProcessService>();

        services.AddSingleton<IIPInfoService, IPInfoService>();
        services.AddSingleton<IQueueService, RedisQueueService>();
        services.AddTransient<ICacheService, RedisCacheService>();
        services.AddTransient<IEmailService, EmailService>();

        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IUserStatsService, UserStatsService>();
        services.AddTransient<IURLService, URLService>();
        services.AddTransient<IShortenerService, ShortenerService>();
        services.AddTransient<IRedirectService, RedirectService>();
        services.AddTransient<IAuthenticationService, AuthenticationService>();
        services.AddTransient<IUserAgentService, UserAgentService>();
        services.AddTransient<UserNotificationService>();

        services.AddScoped<IPaymentService, PaymentService>();
        services.AddKeyedScoped<IPaymentMethod, ZibalPayment>(PaymentTerminals.Zibal);

        return services;
    }

    private static IServiceCollection AddApplicationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        // Application URL and frontend route settings used for outbound links.
        services.AddSettings<ApplicationInfoSettings>(configuration, "ApplicationInfo")
            .Validate(settings => Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out _), "ApplicationInfo:BaseUrl must be an absolute URL.")
            .ValidateOnStart();

        // JWT token generation settings used by token services.
        services.AddSettings<JwtSettings>(configuration, "Jwt")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.TokenSecretKey), "Jwt:TokenSecretKey is required.")
            .ValidateOnStart();

        // ASP.NET Core JWT bearer middleware settings.
        services.AddSettings<JwtBearerSettings>(configuration, "JwtBearer")
            .ValidateOnStart();

        // Auth cookie names and browser behavior.
        services.AddSettings<AuthenticationCookieSettings>(configuration, "AuthenticationCookies")
            .ValidateOnStart();

        // Cross-origin request policy exposed to browsers.
        services.AddSettings<CorsSettings>(configuration, "Cors")
            .Validate(settings => settings.AllowedOrigins.Length > 0, "Cors:AllowedOrigins must contain at least one origin.")
            .ValidateOnStart();

        // Antiforgery header/cookie behavior.
        services.AddSettings<AntiforgerySettings>(configuration, "Antiforgery")
            .ValidateOnStart();

        // Authorization policy-to-role mapping and seeded role definitions.
        services.AddSettings<AuthorizationSettings>(configuration, "Authorization")
            .Validate(settings => settings.Policies.Count > 0, "Authorization:Policies must define at least one policy.")
            .Validate(settings => settings.Roles.Count > 0, "Authorization:Roles must define at least one role.")
            .ValidateOnStart();

        // ASP.NET Core Identity policy settings.
        services.AddSettings<IdentitySettings>(configuration, "Identity")
            .ValidateOnStart();

        // Rate limiting policy definitions applied by middleware.
        services.AddSettings<RateLimitingSettings>(configuration, "RateLimiting")
            .Validate(settings => settings.Policies.Count > 0, "RateLimiting:Policies must define at least one policy.")
            .ValidateOnStart();

        // SignalR endpoint mapping and authorization behavior.
        services.AddSettings<SignalRSettings>(configuration, "SignalR")
            .ValidateOnStart();

        // Redis connection and cache instance settings.
        services.AddSettings<RedisCacheSettings>(configuration, "RedisCache")
            .ValidateOnStart();

        // SMTP provider settings for transactional email.
        services.AddSettings<SmtpSettings>(configuration, "Smtp")
            .ValidateOnStart();

        // External IP intelligence service configuration.
        services.AddSettings<IpInfoSettings>(configuration, "IPInfo");

        // User agent parsing service endpoint configuration.
        services.AddSettings<UserAgentServiceSettings>(configuration, "UserAgentService")
            .ValidateOnStart();

        // Payment provider integration settings.
        services.AddSettings<ZibalSettings>(configuration, "PaymentProviders:Zibal")
            .ValidateOnStart();

        return services;
    }

    private static OptionsBuilder<T> AddSettings<T>(this IServiceCollection services, IConfiguration configuration, string sectionName)
        where T : class
    {
        return services.AddOptions<T>()
            .Bind(configuration.GetSection(sectionName));
    }

    private static IServiceCollection AddRedisInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<RedisCacheOptions>()
            .Configure<IOptions<RedisCacheSettings>>((options, redisSettings) =>
            {
                var settings = redisSettings.Value;
                options.ConfigurationOptions = CreateRedisConfiguration(settings);
                options.InstanceName = settings.InstanceName;
            });

        services.AddStackExchangeRedisCache(_ => { });
        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<RedisCacheSettings>>().Value;
            return ConnectionMultiplexer.Connect(CreateRedisConfiguration(settings));
        });

        return services;
    }

    private static ConfigurationOptions CreateRedisConfiguration(RedisCacheSettings settings)
    {
        var options = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            Password = string.IsNullOrWhiteSpace(settings.Password) ? null : settings.Password
        };

        options.EndPoints.Add(settings.Host, settings.Port);
        return options;
    }
}
