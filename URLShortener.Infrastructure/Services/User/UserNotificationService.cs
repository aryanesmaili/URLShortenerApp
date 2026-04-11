using Microsoft.AspNetCore.SignalR;
using URLShortener.Application.Utility.SignalR;
using URLShortener.Common.SignalR;

namespace URLShortener.Infrastructure.Services.User;

public sealed class UserNotificationService
{
    private readonly IHubContext<UserHub, ISignalRUserClient> _hub;

    public UserNotificationService(IHubContext<UserHub, ISignalRUserClient> hub)
    {
        _hub = hub;
    }

    public async Task NotifyUrlCountChanged(long userId, int count)
    {
        await _hub.Clients
            .Group(GetUserGroupName(userId))
            .ReceiveURLCountUpdate(count);
    }

    public async Task NotifyBalanceChanged(long userId, long newBalance)
    {
        await _hub.Clients
            .Group(GetUserGroupName(userId))
            .ReceiveBalanceUpdate(newBalance);
    }

    private static string GetUserGroupName(long userId) => $"user:{userId}";
}