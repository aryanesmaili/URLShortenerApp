using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using URLShortener.Application;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Interfaces.Infrastructure.External;
using URLShortener.Application.Interfaces.Services.Request;
using URLShortener.Application.Interfaces.Services.URL;
using URLShortener.Application.Interfaces.Services.User;
using URLShortener.Infrastructure.BackgroundServices;
using URLShortener.Infrastructure.Services.Infra;
using URLShortener.Infrastructure.Services.URL;
using URLShortener.Infrastructure.Services.User;

namespace URLShortener.Infrastructure
{
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
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IURLService, URLService>();
            services.AddTransient<IShortenerService, ShortenerService>();
            services.AddTransient<IRedirectService, RedirectService>();

            services.AddTransient<ICacheService, RedisCacheService>();

            services.AddTransient<IUserAgentService, UserAgentService>();
            services.AddTransient<IZibalService, ZibalService>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<UserNotificationService>();

            // Register Mappers By Scanning the Marker Class
            services.AddAutoMapper(
                cfg => { cfg.LicenseKey = "..."; },
                typeof(ApplicationAssemblyMarker));   // Type used to scan the assembly

            // Register Validators By Scanning the Marker Class
            services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

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
}
