using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using URLShortener.Application;
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

public static class RegisterServiceImplementations
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Register Background services
        services.AddHostedService<ClickProcessService>();

        // Register Service Implementations
        services.AddSingleton<IIPInfoService, IPInfoService>();
        services.AddSingleton<IQueueService, RedisQueueService>();

        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IUserStatsService, UserStatsService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IURLService, URLService>();
        services.AddTransient<IShortenerService, ShortenerService>();
        services.AddTransient<IRedirectService, RedirectService>();

        services.AddTransient<ICacheService, RedisCacheService>();

        services.AddTransient<IUserAgentService, UserAgentService>();
        services.AddTransient<UserNotificationService>();

        // Register Mappers By Scanning the Marker Class
        services.AddAutoMapper(
            cfg => { cfg.LicenseKey = "..."; },
            typeof(ApplicationAssemblyMarker));   // Type used to scan the assembly

        // Register Validators By Scanning the Marker Class
        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

        // Register Different Payment Providers
        services.RegisterPayments();

        return services;
    }

    private static IServiceCollection RegisterPayments(this IServiceCollection services)
    {

        services.AddScoped<IPaymentService, PaymentService>();
        services.AddKeyedScoped<IPaymentMethod, ZibalPayment>(PaymentTerminals.Zibal);

        return services;
    }

    public static IServiceCollection RegisterCreds(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis Connection Information binding
        services.AddBinding<RedisConnectionCreds>(configuration, "RedisCacheCreds");

        // Creds needed to contact IPInfo Service
        services.AddBinding<IPInfoCreds>(configuration, "IPInfoCreds");

        // Creds needed to work with user agent service
        services.AddBinding<UserAgentServiceCreds>(configuration, "UserAgentServiceCreds");

        // Creds needed for ZibalPayment to work
        services.AddBinding<ZibalSettings>(configuration, "PaymentProviders:Zibal");

        return services;
    }

    private static IServiceCollection AddBinding<T>(this IServiceCollection services, IConfiguration configuration, string keyName)
        where T : class
    {
        var item = configuration.GetSection(keyName).Get<T>()
            ?? throw new InvalidOperationException($"Configuration section '{keyName}' could not be bound to {typeof(T).Name}");
        services.AddSingleton(item);

        return services;
    }

}
