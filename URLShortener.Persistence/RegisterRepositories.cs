using Microsoft.Extensions.DependencyInjection;
using URLShortener.Application.Repositories;
using URLShortener.Persistence.Repositories;

namespace URLShortener.Persistence;

public static class RegisterRepositories
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register Repository Implementation
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddScoped<IServiceTariffRepository, ServiceTariffRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
