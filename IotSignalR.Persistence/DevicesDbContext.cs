using IotSignalR.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IotSignalR.Persistence;

public class DevicesDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public DevicesDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to sqlite database
        options.UseSqlite(_configuration.GetConnectionString("DevicesDatabase"));
    }

    public DbSet<ManagedDevice> ManagedDevices { get; set; }
}