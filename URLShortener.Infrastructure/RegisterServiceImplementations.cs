using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using URLShortener.Application;
using URLShortener.Application.Interfaces.Infrastructure.External;
using URLShortener.Application.Interfaces.Services.Request;
using URLShortener.Application.Interfaces.Services.URL;
using URLShortener.Application.Interfaces.Services.User;
using URLShortener.Application.Utility.SignalR;
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
            services.AddSingleton<UserConnectionMapping>();

            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IURLService, URLService>();
            services.AddTransient<IShortenerService, ShortenerService>();
            services.AddTransient<IRedirectService, RedirectService>();
            services.AddTransient<ICacheService, CacheService>();
            services.AddTransient<IUserAgentService, UserAgentService>();
            services.AddTransient<IZibalService, ZibalService>();
            services.AddTransient<IPaymentService, PaymentService>();


            // Register Mappers By Scanning the Marker Class
            services.AddAutoMapper(
                cfg => { cfg.LicenseKey = "..."; },
                typeof(ApplicationAssemblyMarker));   // Type used to scan the assembly

            // Register Validators By Scanning the Marker Class
            services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

            return services;
        }
    }
}
