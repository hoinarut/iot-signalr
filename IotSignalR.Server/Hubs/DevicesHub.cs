using IotSignalR.Core.Models;
using Microsoft.AspNetCore.SignalR;

namespace IotSignalR.Server.Hubs
{
    public static class ConnectionsHandler
    {
        public static readonly List<ManagedDevice> Devices = new();
    }

    public class DevicesHub : Hub
    {
        public override async Task<Task> OnConnectedAsync()
        {
            var isDevice = bool.Parse(Context.GetHttpContext()?.Request.Query["isDevice"] ?? false.ToString());
            Console.WriteLine($"Connection from {Context.ConnectionId} is {(isDevice ? "device" : "manager")}");
            if (isDevice)
            {
                var deviceId = Context.GetHttpContext()?.Request.Query["deviceId"] ??
                               throw new InvalidOperationException(
                                   $"Could not retrieve the device id for client {Context.ConnectionId}");
                var device = new ManagedDevice
                {
                    ConnectionId = Context.ConnectionId,
                    IsManager = false,
                    DeviceId = deviceId,
                    LastPollTime = DateTime.UtcNow
                };
                ConnectionsHandler.Devices.Add(device);
                await Clients.Group("Manager").SendAsync("DeviceConnected", device);
            }

            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var device = ConnectionsHandler.Devices.FirstOrDefault(s => s.ConnectionId == Context.ConnectionId);
            if (device is not null)
            {
                await Clients.Group("Manager").SendAsync("DeviceDisconnected", device.DeviceId);
                ConnectionsHandler.Devices.Remove(device);
            }

            Console.WriteLine($"connection from {Context.ConnectionId} disconnected");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Heartbeat(string deviceId)
        {
            var device = ConnectionsHandler.Devices.FirstOrDefault(s => s.DeviceId == deviceId);
            if (device is not null)
            {
                var deviceIndex = ConnectionsHandler.Devices.IndexOf(device);
                device.LastPollTime = DateTime.UtcNow;
                ConnectionsHandler.Devices[deviceIndex] = device;
                await Clients.Group("Manager")
                    .SendAsync("OnHeartbeat", new HeartbeatEventPayload
                    {
                        DeviceId = deviceId,
                        TimeStamp = device.LastPollTime
                    });
            }
        }

        public async Task RegisterManager()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Manager");
            var device = new ManagedDevice
            {
                ConnectionId = Context.ConnectionId,
                IsManager = true,
                DeviceId = Guid.NewGuid().ToString()
            };
            ConnectionsHandler.Devices.Add(device);
        }

        public Task<List<ManagedDevice>> GetAllDevices()
        {
            return Task.FromResult(ConnectionsHandler.Devices.Where(s => s.IsManager == false).ToList());
        }
    }
}