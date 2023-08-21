using System.ComponentModel.DataAnnotations;

namespace IotSignalR.Core.Models;

public class ManagedDevice
{
    [Key]
    public string DeviceId { get; set; }
    public string ConnectionId { get; set; }
    public DateTime LastPollTime { get; set; }
    public bool IsManager { get; set; }
    public bool IsConnected { get; set; }
}