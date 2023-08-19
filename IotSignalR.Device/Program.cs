using IotSignalR.Device;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((hostContext, services) =>
{
    services.AddSingleton<App>();
});
var host = builder.Build();
var app = ActivatorUtilities.CreateInstance<App>(host.Services);
await app.Run();