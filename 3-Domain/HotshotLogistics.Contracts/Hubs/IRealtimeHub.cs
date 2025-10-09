using HotshotLogistics.Contracts.Models;

namespace HotshotLogistics.Contracts.Hubs;

/// <summary>
/// Interface defining SignalR hub methods for real-time communication
/// </summary>
public interface IRealtimeHub
{
    // Server to Client Events
    Task JobStatusUpdated(string jobId, JobStatus status);
    Task LocationUpdated(string jobId, LocationUpdate location);
    Task DriverStatusChanged(int driverId, DriverStatus status);
    Task NewJobAvailable(JobDto job);
    Task NotificationReceived(NotificationMessage message);
}

/// <summary>
/// Interface defining client methods that can be called from the hub
/// </summary>
public interface IRealtimeHubClient
{
    Task JobStatusUpdated(string jobId, JobStatus status);
    Task LocationUpdated(string jobId, LocationUpdate location);
    Task DriverStatusChanged(int driverId, DriverStatus status);
    Task NewJobAvailable(JobDto job);
    Task NotificationReceived(NotificationMessage message);
}



/// <summary>
/// Notification message model for real-time notifications
/// </summary>
public class NotificationMessage
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}





