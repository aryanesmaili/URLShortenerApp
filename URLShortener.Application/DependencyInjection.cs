using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace URLShortener.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(ApplicationAssemblyMarker).Assembly);
        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

        return services;
    }
}
