using System.Net;
using IotSignalR.Core;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IotSignalR.Device
{
    public class App : IAsyncDisposable
    {
        private readonly ILogger<App> _logger;
        private readonly HubConnection _hubConnection;
        private readonly PeriodicTimer _timer;
        private readonly string _deviceId;

        public App(IConfiguration configuration, ILogger<App> logger)
        {
            _logger = logger;
            var clientNumber = configuration.GetValue<int>("ClientNumber");
            _deviceId = Utilities.GetHash($"{Dns.GetHostName()}_{clientNumber}");
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{configuration["DevicesHubUrl"]}?isDevice=true&deviceId={_deviceId}")
                .Build();
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(configuration.GetValue<int>("HeartbeatIntervalSeconds")));
        }

        public async Task Run()
        {
            await _hubConnection.StartAsync();
            while (await _timer.WaitForNextTickAsync())
            {
                await _hubConnection.SendAsync("Heartbeat", _deviceId);
                _logger.LogInformation("Sent Heartbeat at {Time}", DateTimeOffset.UtcNow);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}