using Microsoft.Extensions.Options;
using URLShortener.Application.Common.SignalR;
using URLShortener.Application.Configuration;

namespace URLShortenerAPI;

public static class EndpointMappingExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var signalRSettings = app.Services.GetRequiredService<IOptions<SignalRSettings>>().Value;
        var userHubEndpoint = app.MapHub<UserHub>(signalRSettings.UserHubPath);

        if (signalRSettings.RequireAuthorization && !string.IsNullOrWhiteSpace(signalRSettings.AuthorizationPolicyName))
            userHubEndpoint.RequireAuthorization(signalRSettings.AuthorizationPolicyName);

        return app;
    }
}
