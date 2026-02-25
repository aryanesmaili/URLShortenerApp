using Microsoft.AspNetCore.SignalR;
using URLShortener.Application.Interfaces.Services.User;
using URLShortenerAPI.Utility.SignalR;

namespace URLShortener.Application.Utility.SignalR
{
    public class UserBalanceHub : Hub
    {
        private readonly UserConnectionMapping _userConnections;
        private readonly IUserService _userService;

        public UserBalanceHub(UserConnectionMapping connectionMapping, IUserService userService)
        {
            _userConnections = connectionMapping;
            _userService = userService;
        }

        public override async Task OnConnectedAsync()
        {
            string? userName = Context.User?.Identity?.Name;
            int userID = int.Parse(Context.GetHttpContext()?.Request.Query["userID"].ToString()!);

            _userConnections.AddConnection($"b_{userName}", Context.ConnectionId);

            double userBalance = await _userService.GetUserBalance(userID, userName!);

            await Clients.Caller.SendAsync("ReceiveBalanceUpdate", userBalance);
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            string? userName = Context.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(userName))
                _userConnections.RemoveConnection($"b_{userName}", Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }
    }
}
