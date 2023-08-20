using Microsoft.AspNetCore.SignalR;

namespace IotSignalR.Server.Hubs
{
    public static class ConnectionsHandler
    {
        public static HashSet<string> ConnectedIds = new();
    }

    public class DevicesHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            var isDevice = bool.Parse(Context.GetHttpContext()?.Request.Query["isDevice"] ?? false.ToString());
            Console.WriteLine($"Connection from {Context.ConnectionId} is {(isDevice ? "device" : "manager")}");
            if (isDevice)
            {
                ConnectionsHandler.ConnectedIds.Add(Context.ConnectionId);
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            ConnectionsHandler.ConnectedIds.Remove(Context.ConnectionId);
            Console.WriteLine($"connection from {Context.ConnectionId} disconnected");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task Heartbeat(string clientId)
        {
            await Clients.Group("Manager").SendAsync("OnHeartbeat", $"Received heartbeat from {clientId} at {DateTime.UtcNow}");
        }

        public async Task RegisterManager()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Manager");
            ConnectionsHandler.ConnectedIds.Remove(Context.ConnectionId);
        }

        public Task<List<string>> GetAllDevices()
        {
            return Task.FromResult(ConnectionsHandler.ConnectedIds.ToList());
        }
    }
}
