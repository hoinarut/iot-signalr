using Microsoft.AspNetCore.SignalR;

namespace IotSignalR.Server.Hubs
{
    public class DevicesHub : Hub
    {
        public async Task Heartbeat(string clientId)
        {
            await Clients.Group("Manager").SendAsync("OnHeartbeat", $"Received heartbeat from {clientId} at {DateTime.UtcNow}");
        }

        public async Task RegisterManager()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Manager");
        }
    }
}
