using System.Net;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace IotSignalR.Manager
{
    public class App : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;

        public App(IConfiguration configuration)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(configuration["DevicesHubUrl"])
                .Build();
        }
        public async Task Run()
        {
            _hubConnection.On<string>("OnHeartbeat", Console.WriteLine);
            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync("RegisterManager");
            Console.ReadLine();
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
