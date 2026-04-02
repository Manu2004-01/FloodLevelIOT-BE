using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs
{
    /// <summary>
    /// SignalR hub for real-time sensor reading broadcasts.
    /// Clients subscribe to "ReceiveSensorReading" event.
    /// </summary>
    public class SensorHub : Hub { }
}
