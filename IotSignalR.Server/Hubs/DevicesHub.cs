using IotSignalR.Core.Models;
using IotSignalR.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IotSignalR.Server.Hubs
{
    public class DevicesHub : Hub
    {
        private readonly ILogger<DevicesHub> _logger;
        private readonly DevicesDbContext _dbContext;

        public DevicesHub(ILogger<DevicesHub> logger,
            DevicesDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public override async Task<Task> OnConnectedAsync()
        {
            var isDevice = bool.Parse(Context.GetHttpContext()?.Request.Query["isDevice"] ?? false.ToString());
            Console.WriteLine($"Connection from {Context.ConnectionId} is {(isDevice ? "device" : "manager")}");
            if (isDevice)
            {
                var deviceId = Context.GetHttpContext()?.Request.Query["deviceId"].ToString() ??
                               throw new InvalidOperationException(
                                   $"Could not retrieve the device id for client {Context.ConnectionId}");
                var device = _dbContext.ManagedDevices.FirstOrDefault(d => d.DeviceId == deviceId);
                if (device == null)
                {
                    device = new ManagedDevice
                    {
                        ConnectionId = Context.ConnectionId,
                        IsManager = false,
                        DeviceId = deviceId,
                        LastPollTime = DateTime.UtcNow,
                        IsConnected = true
                    };
                    _dbContext.ManagedDevices.Add(device);
                }
                else
                {
                    device.ConnectionId = Context.ConnectionId;
                    device.IsConnected = true;
                    device.LastPollTime = DateTime.UtcNow;
                    _dbContext.ManagedDevices.Update(device);
                }

                await _dbContext.SaveChangesAsync();
                await Clients.Group("Manager").SendAsync("DeviceConnected", device);
            }

            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var device = _dbContext.ManagedDevices.FirstOrDefault(s => s.ConnectionId == Context.ConnectionId);
            if (device is not null)
            {
                device.IsConnected = false;
                _dbContext.ManagedDevices.Update(device);
                await _dbContext.SaveChangesAsync();
                await Clients.Group("Manager").SendAsync("DeviceDisconnected", device);
            }

            Console.WriteLine($"connection from {Context.ConnectionId} disconnected");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Heartbeat(string deviceId)
        {
            var device = _dbContext.ManagedDevices.FirstOrDefault(s => s.DeviceId == deviceId);
            if (device is not null)
            {
                device.LastPollTime = DateTime.UtcNow;
                _dbContext.ManagedDevices.Update(device);
                await _dbContext.SaveChangesAsync();
                await Clients.Group("Manager")
                    .SendAsync("OnHeartbeat", new HeartbeatEventPayload
                    {
                        DeviceId = deviceId,
                        TimeStamp = device.LastPollTime
                    });
            }
            else
            {
                _logger.LogWarning("Received Heartbeat from unknown device {DeviceId}", deviceId);
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
            _dbContext.ManagedDevices.Add(device);
            await _dbContext.SaveChangesAsync();
        }

        public Task<List<ManagedDevice>> GetAllDevices()
        {
            return _dbContext.ManagedDevices.Where(s => s.IsManager == false).ToListAsync();
        }
    }
}