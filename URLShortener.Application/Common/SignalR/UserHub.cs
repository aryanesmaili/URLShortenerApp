using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using URLShortener.Application.Features.Finance.Interfaces.Services;
using URLShortener.Common.SignalR;

namespace URLShortener.Application.Common.SignalR;

public sealed class UserHub : Hub<ISignalRUserClient>
{
    private readonly IPaymentService _paymentService;

    public UserHub(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroupName(userId));
        }

        long userBalance = await _paymentService.GetUserBalance(int.Parse(userId));
        await Clients.Caller.ReceiveBalanceUpdate(userBalance);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetUserGroupName(userId));
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendBalanceUpdate(long balance)
    {
        var userId = GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.Group(GetUserGroupName(userId)).ReceiveBalanceUpdate(balance);
        }
    }

    private string GetUserId() =>
            (Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
            ?? throw new ArgumentException("UserID not Found in Claims"))
            .Value;

    private static string GetUserGroupName(string userId) => $"user:{userId}";
}
