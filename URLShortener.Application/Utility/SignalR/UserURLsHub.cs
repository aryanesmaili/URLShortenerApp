using Microsoft.AspNetCore.SignalR;
using URLShortener.Application.Interfaces.Services.User;

namespace URLShortener.Application.Utility.SignalR
{
    public class UserURLsHub : Hub
    {
        private readonly UserConnectionMapping _userConnections;
        private readonly IUserService _userService;

        public UserURLsHub(UserConnectionMapping connectionMapping, IUserService userService)
        {
            _userConnections = connectionMapping;
            _userService = userService;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;

            _userConnections.AddConnection($"u_{userName}", Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(userName))
                _userConnections.RemoveConnection($"u_{userName}", Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }
    }
}
