using Microsoft.AspNetCore.SignalR;
using URLShortener.Application.Utility.SignalR;
using URLShortener.Common.SignalR;

namespace URLShortener.Infrastructure.Services.User;

public class UserNotificationService
{
    private readonly IHubContext<UserHub, ISignalRUserClient> _hub;

    public UserNotificationService(IHubContext<UserHub, ISignalRUserClient> hub)
    {
        _hub = hub;
    }

    public async Task NotifyUrlCountChanged(int userId, int count)
    {
        await _hub.Clients
            .Group(GetUserGroupName(userId))
            .ReceiveURLCountUpdate(count);
    }

    public async Task NotifyBalanceChanged(int userId, long newBalance)
    {
        await _hub.Clients
            .Group(GetUserGroupName(userId))
            .ReceiveBalanceUpdate(newBalance);
    }

    private static string GetUserGroupName(int userId) => $"user:{userId}";
}