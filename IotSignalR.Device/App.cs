using System.Net;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace IotSignalR.Device
{
    public class App : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;
        private readonly PeriodicTimer _timer;
        private readonly int _clientNumber;

        public App(IConfiguration configuration)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{configuration["DevicesHubUrl"]}?isDevice=true")
                .Build();
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(configuration.GetValue<int>("HeartbeatIntervalSeconds")));
            _clientNumber = configuration.GetValue<int>("ClientNumber");
        }
        public async Task Run()
        {
            await _hubConnection.StartAsync();
            while (await _timer.WaitForNextTickAsync())
            {
                await _hubConnection.SendAsync("Heartbeat", $"{Dns.GetHostName()}_{_clientNumber}");
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
