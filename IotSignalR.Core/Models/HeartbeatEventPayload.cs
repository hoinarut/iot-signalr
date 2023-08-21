namespace IotSignalR.Core.Models;

public class HeartbeatEventPayload
{
    public string DeviceId { get; set; }
    public DateTime TimeStamp { get; set; }
}